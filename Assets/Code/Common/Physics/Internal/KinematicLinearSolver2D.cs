using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.Common.Physics.Internal
{
    /*
    Collide and slide style solver for 2D that works in linear steps.

    Assumes always upright bounding box, with kinematic rigidbody.
    Basically, this class is all about projecting rigidbody along desired delta,
    taking skin width, surface collisions, and attached colliders into account.
    */
    internal sealed class KinematicLinearSolver2D
    {
        // todo: replace with struct and store as `ref readonly struct` when we finally get C#11
        public record Params
        {
            public int   MaxMoveIterations    { get; set; }
            public int   MaxOverlapIterations { get; set; }
            public float MaxSlopeAngle        { get; set; }
            public bool  VisualizePath        { get; set; }
        }

        private Params _params;
        private KinematicRigidbody2D _body;
        private CollisionFlags2D _collisions;
                

        [Pure]
        private static bool ApproximatelyZero(Vector2 delta)
        {
            // Since a movement amount can be exceedingly tiny depending on the timestep/world-scale/frame-rate,
            // it's more reliable to consider it zero when within floating-point tolerances, rather than custom amounts.
            // Specifically, Vector2 equality check handles this far better than comparing squares of magnitude/Mathf.Epsilon.
            return delta == Vector2.zero;
        }
        
        public override string ToString() =>
            $"{GetType()}, " +
                $"Params: {_params}" +
            $")";

        public KinematicLinearSolver2D(KinematicRigidbody2D body, in Params solverParams)
        {
            if (body == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicRigidbody2D)}");
            }

            _body       = body;
            _collisions = CollisionFlags2D.None;
            _params     = solverParams;
        }


        /*
        Move body given amount.
        
        Notes
        - Interpolation is supported
        - Assumes all collisions are with static (non-moving) objects
        - For flexibility, any external movement such as gravity must be accounted for in given delta
        - Movement is only opted-out if within floating point tolerances of zero, as anything larger will lead to
          skipping movement when deltas are small due to the timestep/world-scale/frame-rate used to compute it prior
         */
        public void SolveMovement(Vector2 deltaPosition)
        {
            SnapToSurfaceIfNearOrInside();
            if (ApproximatelyZero(deltaPosition))
            {
                _collisions = _body.CheckSides();
                return;
            }

            // scale deltas in proportion to the y-axis
            Vector2 up         = _body.Up;
            Vector2 vertical   = Vector2.Dot(deltaPosition, up) * up;
            Vector2 horizontal = deltaPosition - vertical;
            Vector2 position   = _body.Position;

            // note that we resolve horizontal first as the movement is simpler than vertical
            MoveHorizontal(horizontal);
            MoveVertical(vertical);

            SnapToSurfaceIfNearOrInside();
            _collisions = _body.CheckSides();
            _body.MovePosition(startPositionThisFrame: position, targetPositionThisFrame: _body.Position);
        }

        public bool InContact(CollisionFlags2D flags)
        {
            return (_collisions & flags) == flags;
        }

        private void SnapToSurfaceIfNearOrInside()
        {
            for (int i = 0; i < _params.MaxOverlapIterations; i++)
            {
                _body.CheckForOverlappingColliders(out ReadOnlySpan<Collider2D> colliders);
                for (int k = 0; k < colliders.Length; k++)
                {
                    _body.MoveBy(ComputeContactAdjustment(colliders[k]));
                }
            }
        }

        private void MoveHorizontal(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                ProjectAABBAlongDelta(delta, out float stepDistance, out RaycastHit2D hit);
                _body.MoveBy(stepDistance * delta);

                float distanceLeft = Mathf.Max(delta.magnitude - stepDistance, 0f);
                delta = distanceLeft * delta.normalized;

                // unless there's an overly steep slope, move a linear step with properties taken into account
                if (hit && Vector2.Angle(Vector2.up, hit.normal) <= _params.MaxSlopeAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                if (hit)
                {
                    _body.MoveBy(ComputeContactAdjustment(hit.collider));
                }
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                ProjectAABBAlongDelta(delta, out float stepDistance, out RaycastHit2D hit);
                _body.MoveBy(stepDistance * delta);

                float distanceLeft = Mathf.Max(delta.magnitude - stepDistance, 0f);
                delta = distanceLeft * delta.normalized;

                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                if (hit && Vector2.Angle(Vector2.up, hit.normal) > _params.MaxSlopeAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                if (hit)
                {
                    _body.MoveBy(ComputeContactAdjustment(hit.collider));
                }
            }
        }
        
        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        private void ProjectAABBAlongDelta(Vector2 delta, out float distance, out RaycastHit2D hit)
        {
            if (delta == Vector2.zero)
            {
                hit = default;
                distance = 0f;
                return;
            }

            if (!_body.CastAABB_Closest(delta, out hit))
            {
                distance = delta.magnitude;
            }
            else
            {
                distance = hit.distance;
            }
        }


        /*
        Compute the inverse of the minimum separation between given collider and body.
        */
        private Vector2 ComputeContactAdjustment(Collider2D collider)
        {
            // if sufficiently outside the collider, then no adjustment is needed
            Vector2 initialPosition = _body.Position;
            ColliderDistance2D initialSeparation = _body.ComputeMinimumSeparation(collider);
            if (initialSeparation.distance > _body.SkinWidth)
            {
                return Vector2.zero;
            }
            // otherwise, move the entire distance needed to resolve the initial overlap
            _body.MoveBy(initialSeparation.distance * initialSeparation.normal);

            // in the case of a convex collider, we may need additional small adjustments to find a non-overlapping spot
            for (int i = 0; i < _params.MaxOverlapIterations; i++)
            {
                ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(collider);

                Vector2 offset = minimumSeparation.distance * minimumSeparation.normal;
                if (ApproximatelyZero(offset))
                {
                    break;
                }
                _body.MoveBy(offset);
            }

            // now that the final position was found, return the rigidbody back to its original state
            Vector2 finalPosition = _body.Position;
            _body.MoveTo(initialPosition);
            return finalPosition - initialPosition;
        }

        /*
        Apply bounciness/friction coefficients to hit position/normal, in proportion with the desired movement distance.

        In other words, for a given collision what is the adjusted delta when taking impact angle, velocity, bounciness,
        and friction into account (using a linear model similar to Unity's dynamic physics)?
        
        Note that collisions are resolved via: adjustedDelta = moveDistance * [(Sbounciness)Snormal + (1-Sfriction)Stangent]
        * where bounciness is from 0 (no bounciness) to 1 (completely reflected)
        * friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance)
        */
        private Vector2 ComputeCollisionDelta(Vector2 delta, Vector2 hitNormal, float bounciness=0f, float friction=0f)
        {
            float remainingDistance = delta.magnitude;
            Vector2 reflected  = Vector2.Reflect(delta, hitNormal);
            Vector2 projection = Vector2.Dot(reflected, hitNormal) * hitNormal;
            Vector2 tangent    = reflected - projection;

            Vector2 perpendicularContribution = (bounciness      * remainingDistance) * projection.normalized;
            Vector2 tangentialContribution    = ((1f - friction) * remainingDistance) * tangent.normalized;
            return perpendicularContribution + tangentialContribution;
        }
    }
}

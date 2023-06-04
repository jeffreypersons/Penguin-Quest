using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using PQ.Common.Extensions;


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
            _body.MovePosition(startPositionThisFrame: position, targetPositionThisFrame: _body.Position);

            _collisions = _body.CheckSides();
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
                    ResolveOverlapWithCollider(colliders[k]);
                }
            }
        }

        private void MoveHorizontal(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                MoveAABBAlongDelta(ref delta, out RaycastHit2D hit);

                // unless there's an overly steep slope, move a linear step with properties taken into account
                if (hit && Vector2.Angle(Vector2.up, hit.normal) <= _params.MaxSlopeAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                ResolveOverlapWithCollider(hit.collider);
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                MoveAABBAlongDelta(ref delta, out RaycastHit2D hit);

                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                if (hit && Vector2.Angle(Vector2.up, hit.normal) > _params.MaxSlopeAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                ResolveOverlapWithCollider(hit.collider);
            }
        }
        

        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        private void MoveAABBAlongDelta(ref Vector2 delta, out RaycastHit2D obstruction)
        {
            float   distanceLeft       = delta.magnitude;
            Vector2 direction          = delta / distanceLeft;
            float   distanceToAABBEdge = _body.ComputeDistanceToEdge(direction);

            obstruction = default;
            float stepDistance = Mathf.Min(distanceToAABBEdge, distanceLeft);
            if (_body.CastAABB(direction, stepDistance, out ReadOnlySpan<RaycastHit2D> hits, includeAlreadyOverlappingColliders: true))
            {
                obstruction  = hits[0];
                stepDistance = hits[0].distance;
            }

            Vector2 step = stepDistance * direction;

            #if UNITY_EDITOR
            Vector2 origin = _body.Position;
            Vector2 max = distanceLeft * direction;
            float duration = Time.fixedDeltaTime;

            DebugExtensions.DrawLine(origin, origin + delta, Color.white, duration);
            DebugExtensions.DrawRayCast(origin, step, hits.IsEmpty? default: hits[0], duration);
            #endif

            _body.MoveBy(step);
            delta -= step;
        }


        /*
        Compute the inverse of the minimum separation between given collider and body.
        */
        private void ResolveOverlapWithCollider(Collider2D collider)
        {
            if (collider == null)
            {
                return;
            }

            // if sufficiently outside the collider, then no adjustment is needed
            ColliderDistance2D initialSeparation = _body.ComputeMinimumSeparation(collider);
            if (initialSeparation.distance > _body.SkinWidth)
            {
                return;
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

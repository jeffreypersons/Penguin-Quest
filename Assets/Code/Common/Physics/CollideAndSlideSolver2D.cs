using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Collide and slide style solver for 2D that works in linear steps.

    Assumes always upright bounding box, with kinematic rigidbody.
    Basically, this class is all about projecting rigidbody along desired delta,
    taking skin width, surface collisions, and attached colliders into account.
    */
    public sealed class CollideAndSlideSolver2D
    {
        private SolverParams _params;
        private KinematicBody2D _body;
        private CollisionFlags2D _collisions;

        public SolverParams     Params => _params;
        public CollisionFlags2D Flags  => _collisions;

        public override string ToString() =>
            $"{GetType()}, " +
                $"Flags: {_collisions}," +
                $"Params: {_params}," +
                $"Body: {_body}," +
            $")";

        [Pure]
        private static bool ApproximatelyZero(Vector2 delta)
        {
            // Since a movement amount can be exceedingly tiny depending on the timestep/world-scale/frame-rate,
            // it's more reliable to consider it zero when within floating-point tolerances, rather than custom amounts.
            // Specifically, Vector2 equality check handles this far better than comparing squares of magnitude/Mathf.Epsilon.
            return delta == Vector2.zero;
        }

        public CollideAndSlideSolver2D(KinematicBody2D body, in SolverParams solverParams)
        {
            if (body == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicBody2D)}");
            }

            _body       = body;
            _params     = solverParams;
            _collisions = CollisionFlags2D.None;
        }

        public void Flip(bool horizontal)
        {
            _body.Flip(horizontal, false);
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
        public void Move(Vector2 deltaPosition)
        {
            if (ApproximatelyZero(deltaPosition))
            {
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
            _collisions = _body.CheckForOverlappingContacts(_body.SkinWidth);

            _body.InterpolatedMoveTo(startPositionThisFrame: position, targetPositionThisFrame: _body.Position);
        }

        public bool InContact(CollisionFlags2D flags)
        {
            return (_collisions & flags) == flags;
        }


        private void MoveHorizontal(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                // move directly to target if unobstructed
                if (!DetectClosestCollision(delta, out RaycastHit2D hit))
                {
                    _body.MoveBy(delta);
                    delta = Vector2.zero;
                    continue;
                }

                // unless there's an overly steep slope, move a linear step with properties taken into account
                if (Vector2.Angle(Vector2.up, hit.normal) <= _params.MaxSlopeAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                PushOutIfOverlap(hit);
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                // move directly to target if unobstructed
                if (!DetectClosestCollision(delta, out RaycastHit2D hit))
                {
                    _body.MoveBy(delta);
                    delta = Vector2.zero;
                    continue;
                }

                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                if (Vector2.Angle(Vector2.up, hit.normal) > _params.MaxSlopeAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                PushOutIfOverlap(hit);
            }
        }


        /*
        Project AABB along delta, and return CLOSEST hit (if any).
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        private bool DetectClosestCollision(Vector2 delta, out RaycastHit2D hit)
        {
            if (!_body.CastAABB(delta, out ReadOnlySpan<RaycastHit2D> hits))
            {
                hit = default;
                return false;
            }

            int closestHitIndex = 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].distance < hits[closestHitIndex].distance)
                {
                    closestHitIndex = i;
                }
            }
            hit = hits[closestHitIndex];
            return true;
        }

        private void PushOutIfOverlap(RaycastHit2D hit)
        {
            // todo: add skin width support
            Vector2 overlapAmount = Vector2.positiveInfinity;
            for (int i = 0; i < _params.MaxOverlapIterations && !ApproximatelyZero(overlapAmount); i++)
            {
                if (_body.ComputeOverlap(hit.collider, out overlapAmount))
                {
                    _body.MoveBy(overlapAmount);
                }
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

            Vector2 perpendicularContribution = (bounciness * remainingDistance) * projection.normalized;
            Vector2 tangentialContribution    = ((1f - friction) * remainingDistance) * tangent.normalized;
            return perpendicularContribution + tangentialContribution;
        }
    }
}

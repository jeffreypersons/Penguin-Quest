using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_002
{
    [Flags]
    public enum CollisionFlags2D
    {
        None   = 0,
        Front  = 1 << 1,
        Below  = 1 << 2,
        Behind = 1 << 3,
        Above  = 1 << 4,
        All    = ~0,
    }
    public sealed class Mover
    {
        private Body _body;
        private float _maxAngle;
        private int _maxMoveIterations;
        private int _maxOverlapIterations;
        private CollisionFlags2D _collisions;


        [Pure]
        private static bool ApproximatelyZero(Vector2 delta)
        {
            // Since a movement amount can be exceedingly tiny depending on the timestep/world-scale/frame-rate,
            // it's more reliable to consider it zero when within floating-point tolerances, rather than custom amounts.
            // Specifically, Vector2 equality check handles this far better than comparing squares of magnitude/Mathf.Epsilon.
            return delta == Vector2.zero;
        }

        [Pure]
        private (float distance, Vector2 direction) DecomposeDelta(Vector2 delta)
        {
            // compute equivalent of (delta.magnitude, delta.normalized) with single sqrt call (benchmarked at ~46% faster)
            // note that the epsilon used below is consistent with Vector2's Normalize()
            float squaredMagnitude = delta.sqrMagnitude;
            if (squaredMagnitude <= 1E-010f)
            {
                return (0f, Vector2.zero);
            }

            float magnitude = Mathf.Sqrt(squaredMagnitude);
            delta /= magnitude;
            return (magnitude, delta);
        }

        [Pure]
        private (float distance, Vector2 direction) ProjectDeltaOnToSurface(Vector2 delta, Vector2 normal)
        {
            // take perpendicular of surface normal in direction of body
            Vector2 surfaceTangent = _body.transform.localEulerAngles.y >= 90
                ? new Vector2(-normal.y, normal.x)
                : new Vector2(normal.y, -normal.x);

            // vector projection of delta onto to surface tangent (2D equivalent of 3D method ProjectOnPlane())
            // assumes non-zero normal (ie given hit is valid)
            float aDotB = Vector2.Dot(delta, surfaceTangent);
            float bDotB = Vector2.Dot(surfaceTangent, surfaceTangent);
            return (aDotB / bDotB, surfaceTangent);
        }

        public Mover(Transform transform)
        {
            _body = transform.GetComponent<Body>();
            _collisions = CollisionFlags2D.None;
            _body.Flip(horizontal: false, vertical: false);
        }

        public void SetParams(float maxSlopeAngle, int maxMoveIterations, int maxOverlapIterations)
        {
            _maxAngle = maxSlopeAngle;
            _maxMoveIterations = maxMoveIterations;
            _maxOverlapIterations = maxOverlapIterations;
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
            for (int i = 0; i < _maxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                // move directly to target if unobstructed
                if (!DetectClosestCollision(delta, out RaycastHit2D hit))
                {
                    _body.MoveBy(delta);
                    delta = Vector2.zero;
                    continue;
                }

                // unless there's an overly steep slope, move a linear step with properties taken into account
                if (Vector2.Angle(Vector2.up, hit.normal) <= _maxAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                SnapToCollider(hit.collider);
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _maxMoveIterations && !ApproximatelyZero(delta); i++)
            {
                // move directly to target if unobstructed
                if (!DetectClosestCollision(delta, out RaycastHit2D hit))
                {
                    _body.MoveBy(delta);
                    delta = Vector2.zero;
                    continue;
                }

                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                if (Vector2.Angle(Vector2.up, hit.normal) > _maxAngle)
                {
                    Vector2 collisionResponse = ComputeCollisionDelta(hit.distance * delta.normalized, hit.normal);
                    _body.MoveBy(collisionResponse);
                }

                SnapToCollider(hit.collider);
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

        private void SnapToCollider(Collider2D collider)
        {
            Vector2 startPosition = _body.Position;
            for (int i = 0; i < _maxOverlapIterations; i++)
            {
                ColliderDistance2D minSeparation = _body.ComputeMinimumSeparation(collider);
                Vector2 offset = minSeparation.distance * minSeparation.normal;
                if (offset == Vector2.zero)
                {
                    break;
                }
                _body.MoveBy(offset);
            }
            Vector2 endPosition = _body.Position;

            if (startPosition != endPosition)
            {
                Vector2 markerExtents = 0.075f * Vector2.Perpendicular(endPosition - startPosition);
                Debug.DrawLine(startPosition - markerExtents, startPosition + markerExtents, Color.red, 10f);
                Debug.DrawLine(startPosition, endPosition, Color.white, 10f);
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

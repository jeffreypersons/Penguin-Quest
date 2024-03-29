using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_001
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
        private int _maxMoveIterations;
        private CollisionFlags2D _collisions;
        
        [Pure]
        private (float distance, Vector2 direction) DecomposeDelta(Vector2 delta)
        {
            // below is exactly equivalent to (delta.normalized, delta.magnitude) without the redundant
            // sqrt call (benchmarks showed ~54% faster), and without NaNs on zero length vectors by just dividing distance
            float squaredMagnitude = delta.sqrMagnitude;
            if (squaredMagnitude <= 1E-010f)
            {
                return (0f, Vector2.zero);
            }

            float magnitude = Mathf.Sqrt(squaredMagnitude);
            delta /= magnitude;
            return (magnitude, delta);
        }


        public Mover(Transform transform)
        {
            _body = transform.GetComponent<Body>();
            _collisions = CollisionFlags2D.None;
            _body.Flip(horizontal: false, vertical: false);
        }

        public void SetParams(int maxMoveIterations)
        {
            _maxMoveIterations = maxMoveIterations;
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
            if (deltaPosition == Vector2.zero)
            {
                // todo: look into adding min separation resolution here for any overlapping colliders
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

            _body.MovePosition(startPositionThisFrame: position, targetPositionThisFrame: _body.Position);

            _collisions = _body.CheckSides();
        }

        public bool InContact(CollisionFlags2D flags)
        {
            return (_collisions & flags) == flags;
        }
        

        private void MoveHorizontal(Vector2 initialDelta)
        {
            (float distanceLeft, Vector2 currentDirection) = DecomposeDelta(initialDelta);
            for (int i = 0; i < _maxMoveIterations; i++)
            {
                if (!MoveAABBAlongDelta(currentDirection, ref distanceLeft, out float step, out RaycastHit2D obstruction))
                {
                    break;
                }
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            (float distanceLeft, Vector2 currentDirection) = DecomposeDelta(initialDelta);
            for (int i = 0; i < _maxMoveIterations; i++)
            {
                if (!MoveAABBAlongDelta(currentDirection, ref distanceLeft, out float step, out RaycastHit2D obstruction))
                {
                    break;
                }
            }
        }

        
        /* Project AABB along delta until (if any) obstruction. Assumes no initial overlaps. Max distance caps at body-radius to prevent tunneling. */
        private bool MoveAABBAlongDelta(Vector2 direction, ref float distanceLeft, out float step, out RaycastHit2D obstruction)
        {
            if (direction == Vector2.zero)
            {
                distanceLeft = 0f;
                step = 0f;
                obstruction = default;
                return false;
            }
            // todo: verify small differences, and that things don't go negative, etc
            float distanceToAABBEdge = _body.ComputeDistanceToEdge(direction);

            // move box along delta a distance no greater than bound-extents, stopping at the first collision (if any)
            obstruction = default;
            step = Mathf.Min(distanceToAABBEdge, distanceLeft);
            if (_body.CastAABB(direction, step, out ReadOnlySpan<RaycastHit2D> hits, false))
            {
                obstruction = hits[0];
                step = hits[0].distance;
            }
            _body.MoveBy(step * direction);

            // ----- disabled for now - causing 'overlap fighting where it goes up and down repeatedly due to incorrect resolution
            // if there was an obstruction, apply any depenetration
            /*if (obstruction && _body.ComputeSeparation(obstruction.collider, direction, out float resolveAmount, out Vector2 resolveDirection, out bool overlapped) && overlapped)
            {
                _body.MoveBy(resolveAmount * resolveDirection);
                step -= resolveAmount;
            }*/

            // if no step to take, there's no more that can move
            Vector2 deltaStep = step * direction;
            if (deltaStep == Vector2.zero)
            {
                step = 0f;
                distanceLeft = 0f;
                return false;
            }
            distanceLeft -= step;
            return true;
        }
    }
}

using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_003
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
    internal sealed class KinematicLinearSolver2D
    {
        private Vector2 _obstructionNormal;
        private KinematicBody2D _body;
        private CollisionFlags2D _collisions;

        // todo: cache any direction dependent data if possible (eg body-radius, projection)

        public KinematicLinearSolver2D(KinematicBody2D kinematicBody2D)
        {
            if (kinematicBody2D == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicLinearSolver2D)}");
            }
            _body = kinematicBody2D;

            _obstructionNormal = Vector2.up;
        }

        public void Flip(bool horizontal, bool vertical)
        {
            Vector3 rotation = new Vector3(
                x: horizontal ? 180f : 0f,
                y: vertical   ? 180f : 0f,
                z: 0f);

            if (_body.Rotation != rotation)
            {
                _body.Rotation = rotation;
            }
        }

        public bool InContact(CollisionFlags2D flags)
        {
            return (_collisions & flags) == flags;
        }

        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        public void Move(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            (float desiredDistance,   Vector2 desiredDirection  ) = DecomposeDelta(delta);
            (float projectedDistance, Vector2 projectedDirection) = ProjectDeltaOnToSurface(delta, _obstructionNormal);
            Debug.DrawRay(_body.Position, _body.Position + (desiredDistance     * desiredDirection), Color.gray,  1f);
            Debug.DrawRay(_body.Position, _body.Position + (projectedDistance * projectedDirection), Color.green, 1f);

            Vector2 startPosition = _body.Position;
            MoveUnobstructed(
                projectedDistance,
                projectedDirection,
                out float step,
                out RaycastHit2D obstruction);
            Vector2 endPosition = _body.Position;
            _body.MovePositionWithoutBreakingInterpolation(startPosition, endPosition);

            _obstructionNormal = obstruction ? obstruction.normal : Vector2.up;
        }


        /* Project body along delta until (if any) obstruction. Distance swept is capped at body-radius to prevent tunneling. */
        private void MoveUnobstructed(float distance, Vector2 direction, out float step, out RaycastHit2D obstruction)
        {
            // slightly bias the start position such that box casts still resolve
            // even when AABB is touching a collider in that direction
            float startOffset = _body.SkinWidth;
            float bodyRadius = _body.ComputeDistanceToEdge(direction);
            _body.Position -= startOffset * direction;

            step = distance < bodyRadius ? distance : bodyRadius;
            if (_body.CastAABB(direction, step + startOffset, out obstruction))
            {
                step = obstruction.distance - startOffset;
            }
            _body.Position += (step + startOffset) * direction;
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
            Vector2 surfaceTangent = _body.Rotation.y >= 90
                ? new Vector2(-normal.y,  normal.x)
                : new Vector2( normal.y, -normal.x);

            // vector projection of delta onto to surface tangent (2D equivalent of 3D method ProjectOnPlane())
            // assumes non-zero normal (ie given hit is valid)
            float aDotB = Vector2.Dot(delta,          surfaceTangent);
            float bDotB = Vector2.Dot(surfaceTangent, surfaceTangent);
            return (aDotB / bDotB, surfaceTangent);
        }
    }
}

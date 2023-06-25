using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.LinearStep_002
{
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;


        public KinematicLinearSolver2D(KinematicBody2D kinematicBody2D)
        {
            if (kinematicBody2D == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicLinearSolver2D)}");
            }
            _body = kinematicBody2D;
        }

        public void MoveUnobstructedAlongDelta(Vector2 delta)
        {
            (float distanceRemaining, Vector2 direction) = DecomposeDelta(delta);
            float startOffset = _body.SkinWidth;
            float distanceToEdge = _body.ComputeDistanceToEdge(direction);

            _body.MoveBy(-startOffset * direction);

            float step = distanceRemaining < distanceToEdge ? distanceRemaining : distanceToEdge;
            if (_body.CastAABB(direction, step + startOffset, out RaycastHit2D obstruction))
            {
                step = obstruction.distance - startOffset;
            }

            _body.MoveBy((step + startOffset) * direction);
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
        private Vector2 ProjectDeltaOnToSurface(Vector2 delta, RaycastHit2D hit)
        {
            // take the perpendicular of the surface normal, aligned with body orientation
            Vector2 surfaceTangent = _body.IsFlippedHorizontal
                ? new Vector2(-hit.normal.y,  hit.normal.x)
                : new Vector2( hit.normal.y, -hit.normal.x);

            // vector projection of delta onto to surface tangent (2D equivelant of 3D method ProjectOnPlane())
            float dot1 = Vector2.Dot(surfaceTangent, surfaceTangent);
            float dot2 = Vector2.Dot(delta,          surfaceTangent);
            return new Vector2((surfaceTangent.x * dot2) / dot1, (surfaceTangent.y * dot2) / dot1);
        }
    }
}

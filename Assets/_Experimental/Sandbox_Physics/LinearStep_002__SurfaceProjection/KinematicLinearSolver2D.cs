using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.LinearStep_002
{
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;

        private float   moveDistance;
        private Vector2 moveDirection;

        public KinematicLinearSolver2D(KinematicBody2D kinematicBody2D)
        {
            if (kinematicBody2D == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicLinearSolver2D)}");
            }
            _body = kinematicBody2D;

            moveDistance  = 0f;
            moveDirection = Vector2.right;
        }

        public void MoveUnobstructedAlongDelta(Vector2 delta)
        {
            // todo: maintain the below distance when we implement momentum
            moveDistance = 0f;

            (float distanceRemaining, Vector2 direction) = DecomposeDelta(delta);
            float startOffset = _body.SkinWidth;
            float distanceToEdge = _body.ComputeDistanceToEdge(direction);

            _body.MoveBy(-startOffset * direction);

            float stepDistance = distanceRemaining < distanceToEdge ? distanceRemaining : distanceToEdge;
            if (_body.CastAABB(direction, stepDistance + startOffset, out RaycastHit2D obstruction))
            {
                stepDistance = obstruction.distance - startOffset;
            }

            // todo: avoid extra project calls
            Vector2 stepDelta = (stepDistance + startOffset) * direction;
            if (obstruction)
            {
                stepDelta = ProjectDeltaOnToSurface(stepDelta, obstruction);
            }

            (moveDistance, moveDirection) = DecomposeDelta(stepDelta);
            _body.MoveBy(stepDelta);
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
            // take perpendicular of surface normal in direction of body
            Vector2 surfaceTangent = _body.IsFlippedHorizontal
                ? new Vector2(-hit.normal.y,  hit.normal.x)
                : new Vector2( hit.normal.y, -hit.normal.x);

            // vector projection of delta onto to surface tangent (2D equivelant of 3D method ProjectOnPlane())
            // assumes non-zero normal (ie given hit is valid)
            float aDotB = Vector2.Dot(delta,          surfaceTangent);
            float bDotB = Vector2.Dot(surfaceTangent, surfaceTangent);
            return (aDotB / bDotB) * surfaceTangent;
        }
    }
}

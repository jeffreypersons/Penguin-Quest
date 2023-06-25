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
            // the below gets behavior exactly consistent with (delta.normalized, delta.magnitude),
            // without extra square root call, and without the NaNs that arise if delta is zero and
            // divided by it's magnitude without epsilon checks (that Unity does in delta.normalized)
            float squaredMagnitude = delta.sqrMagnitude;
            if (squaredMagnitude <= 1E-010f)
            {
                return (0f, Vector2.zero);
            }

            float magnitude = Mathf.Sqrt(squaredMagnitude);
            delta /= magnitude;
            return (magnitude, delta);
        }
    }
}

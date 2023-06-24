using System;
using UnityEngine;


namespace PQ._Experimental.Physics.LinearStep_001
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
            Vector2 direction = delta.normalized;
            float startOffset = _body.SkinWidth;
            float distanceRemaining = delta.magnitude;
            float distanceToEdge = _body.ComputeDistanceToEdge(direction);

            _body.MoveBy(-startOffset * direction);

            float step = distanceRemaining < distanceToEdge ? distanceRemaining : distanceToEdge;
            if (_body.CastAABB(direction, step + startOffset, out RaycastHit2D obstruction))
            {
                step = obstruction.distance - startOffset;
            }

            _body.MoveBy((step + startOffset) * direction);
        }
    }
}

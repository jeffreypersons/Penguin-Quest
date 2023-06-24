using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Overlap_003
{
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;
        private int _maxMinSeparationSolves = 10;


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
            float distance = delta.magnitude;
            Vector2 direction = delta.normalized;

            if (!_body.CastCircle(direction, distance, out RaycastHit2D obstruction, true))
            {
                _body.MoveBy(delta);
                return;
            }

            _body.MoveBy(obstruction.fraction * delta);
        }


        private void SnapToCollider(Collider2D collider)
        {
            Vector2 startPosition = _body.Position;
            for (int i = 0; i < _maxMinSeparationSolves; i++)
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
    }
}

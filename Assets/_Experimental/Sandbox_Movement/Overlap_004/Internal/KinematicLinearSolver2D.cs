using System;
using UnityEngine;


namespace PQ._Experimental.Overlap_004.Internal
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

            MoveCircleAlongDelta();
        }

        private void MoveCircleAlongDelta()
        {

        }
    }
}

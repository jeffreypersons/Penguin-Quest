using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_003
{
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;
        private const int MaxIterations = 10;

        // todo: cache any direction dependent data if possible (eg body-radius, projection)

        public KinematicLinearSolver2D(KinematicBody2D kinematicBody2D)
        {
            if (kinematicBody2D == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicLinearSolver2D)}");
            }
            _body = kinematicBody2D;
        }

        public void Flip(bool horizontal, bool vertical)
        {
            Vector3 rotation = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);

            if (_body.Rotation != rotation)
            {
                _body.Rotation = rotation;
            }
        }

        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        public void Move(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            Vector2 startPosition = _body.Position;

            int iteration = MaxIterations;
            float distanceRemaining = delta.magnitude;
            Vector2 direction = delta.normalized;
            while (iteration-- > 0 && distanceRemaining * direction != Vector2.zero)
            {
                Vector2 beforeStep = _body.Position;
                Debug.Log($"Move({delta}).substep#{MaxIterations-iteration} : remaining={distanceRemaining}, direction={direction}");
                Debug.DrawLine(beforeStep, beforeStep + (distanceRemaining * direction), Color.gray, 1f);

                MoveUnobstructed(
                    distanceRemaining,
                    direction,
                    out float step,
                    out RaycastHit2D obstruction);

                Vector2 afterStep = _body.Position;

                Debug.DrawLine(beforeStep, afterStep, Color.green, 1f);

                direction -= obstruction.normal * Vector2.Dot(direction, obstruction.normal);
                distanceRemaining -= step;
            }

            Vector2 endPosition = _body.Position;

            _body.MovePositionWithoutBreakingInterpolation(startPosition, endPosition);
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
    }
}

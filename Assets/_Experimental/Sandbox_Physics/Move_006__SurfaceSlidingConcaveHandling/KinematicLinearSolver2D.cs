using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_006
{
    /*
    Collide and slide solver for movement.

    Notes
    - Prevents tunneling by clamping an movement sub-step to body extents (allowing backtracking if overlap)
    - Flipping is done only by local rotation along x and y axes - no changes in scale
    - When moving along surfaces, we maintain a slight offset from the normal, such that contacts are
      intentionally avoided. This way, we avoid getting caught on edges and corners 
    */
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;

        /* Number of iterations used to reach movement target before giving up. */
        public const int MaxMoveIterations = 10;

        /* Number of iterations used to reach no overlap before giving up. */
        public const int MaxOverlapIterations = 5;

        /* Amount which we consider to be (close enough to) zero. */
        public const float Epsilon = 0.005f;

        /* Amount used to ensure we don't get _too_ close to surfaces, to avoid getting stuck when moving tangential to a surface. */
        public const float ContactOffset = 0.05f;


        public KinematicLinearSolver2D(KinematicBody2D kinematicBody2D)
        {
            if (kinematicBody2D == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicLinearSolver2D)}");
            }
            _body = kinematicBody2D;
        }


        /*
        Resolve any separation between given body and collider.

        Reposition body to touch collider with no gap or overlap (or until max iterations reached)
        - Safeguards against passing through collider when resolving separation
        - Since separation is solved iteratively in linear steps, complex geometry (ie many concave faces) require more iterations
        */
        public void ResolveSeparation(Collider2D collider)
        {
            int iteration = MaxOverlapIterations;
            ColliderDistance2D separation = _body.ComputeMinimumSeparation(collider);

            // if collider is entered when resolving resolution, then start further out
            // specifically this prevents bodies from snapping to the other side of an edge collider
            if (_body.CastRayAt(collider, _body.Position, separation.normal, separation.distance, out var _))
            {
                Debug.Log($"RemoveOverlap({collider.name}) : Initial resolution caused collider to pass through an edge - pushing back to compensate");
                _body.Position += -2f * _body.ComputeDistanceToEdge(separation.normal) * separation.normal;
            }

            // note that we remove separation if ever so slightly above surface as well
            Vector2 startPosition = _body.Position;
            while (iteration-- > 0 && separation.distance is < -Epsilon or > Epsilon)
            {
                ColliderDistance2D previousSeparation = separation;
                Vector2 beforeStep = _body.Position;
                separation = _body.ComputeMinimumSeparation(collider);

                Debug.Log($"RemoveOverlap({collider.name}).substep#{MaxOverlapIterations - iteration} : " +
                          $"remaining={separation.distance}, direction={separation.normal}");

                if (separation.distance >= previousSeparation.distance)
                {
                    // typically this only occurs on sharp protruding angles where the body can catapult away from surface normal
                    // so any time we are trying to converge the separation to zero, stop as a safeguard
                    Debug.Log($"RemoveOverlap({collider.name}) : Separation amount increased - halting resolution");
                    break;
                }
                _body.Position += separation.distance * separation.normal;

                Vector2 afterStep = _body.Position;
                Debug.DrawLine(beforeStep, afterStep, Color.yellow, 1f);
            }
            Vector2 endPosition = _body.Position;

            // bias the resolved position ever so slightly along the normal to prevent contact
            // note this also prevents infinite flip flopping if body is placed exactly at the center of an overlapping collider
            _body.Position += Epsilon * (endPosition - startPosition).normalized;
        }


        /*
        Set direction of body via local xy axes. Note does not change scale.

        Defaults to right and up, and left and down respectively, if inverted.
        */
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

        /*
        Move body by given change in position, taking surface contacts into account.

        Body is moved along surfaces until either distance, obstruction in opposing direction (ie wall), or max iterations are reached
        - Tunneling is avoided by clamping distance moved per iteration to body extents (allowing backtracking if overlap)
        - Sticking to corners is avoided by maintaining a slight offset from all surface contact normals
        - Steps are done linearly, allowing for an arbitrary surface to be moved along
        */
        public void Move(float distance, Vector2 direction)
        {
            // note that we compare extremely close to zero rather than our larger epsilon,
            // as delta can be very small depending on the physics step duration used to compute it
            if (distance * direction == Vector2.zero)
            {
                return;
            }

            Vector2 startPosition = _body.Position;
            int iteration = MaxMoveIterations;
            while (iteration-- > 0 &&
                   distance > Epsilon &&
                   direction.sqrMagnitude > Epsilon
                   /*&&!(direction == Vector2.down && CheckForConcaveFaceBelow())*/)
            {
                Vector2 beforeStep = _body.Position;
                Debug.DrawLine(beforeStep, beforeStep + (distance * direction), Color.gray, 1f);
                
                Debug.Log($"Move({distance*direction}).substep#{MaxMoveIterations-iteration} : " +
                          $"remaining={distance}, direction={direction}");
                MoveUnobstructed(
                    direction,
                    distance,
                    out float step,
                    out RaycastHit2D obstruction);

                Vector2 afterStep = _body.Position;
                Debug.DrawLine(beforeStep, afterStep, Color.green, 1f);

                direction -= obstruction.normal * Vector2.Dot(direction, obstruction.normal);
                distance -= step;
            }
            Vector2 endPosition = _body.Position;
            _body.MovePositionWithoutBreakingInterpolation(startPosition, endPosition);
        }


        /* Project body along delta until (if any) obstruction. Distance swept is capped at body-radius to prevent tunneling. */
        private void MoveUnobstructed(Vector2 direction, float distance, out float step, out RaycastHit2D obstruction)
        {
            float bodyRadius = _body.ComputeDistanceToEdge(direction);

            step = distance < bodyRadius ? distance : bodyRadius;
            if (_body.CastAABB(direction, step + ContactOffset, out var hit))
            {
                float distancePastOffset = hit.distance - ContactOffset;
                step = distancePastOffset < Epsilon? 0f : distancePastOffset;
                obstruction = hit;
            }
            else
            {
                obstruction = default;
            }

            _body.Position += step * direction;
        }

        private bool CheckForConcaveFaceBelow()
        {
            float bodyRadius = _body.ComputeDistanceToEdge(Vector2.down);

            Vector2 center  = _body.Center;
            Vector2 extents = _body.Extents;

            Vector2 bottomCenter = new Vector2(center.x, center.y - extents.y);
            if (!_body.CastRay(bottomCenter, Vector2.down, bodyRadius, out var middleHit))
            {
                return false;
            }

            Vector2 bottomLeft = new Vector2(center.x - extents.x, center.y - extents.y);
            if (!_body.CastRay(bottomLeft, Vector2.down, middleHit.distance, out var leftHit))
            {
                return false;
            }

            Vector2 bottomRight = new Vector2(center.x + extents.x, center.y - extents.y);
            if (!_body.CastRay(bottomRight, Vector2.down, middleHit.distance, out var rightHit))
            {
                return false;
            }

            Debug.Log($"leftDist={leftHit.distance} middleDist={middleHit.distance} rightDist={rightHit.distance}");
            return middleHit.distance > leftHit.distance && middleHit.distance > rightHit.distance;
        }
        
        private bool CheckForConcaveFaceAlongPath(Vector2 direction, float distance)
        {
            Vector2 center  = _body.Center;
            Vector2 extents = _body.Extents;

            Vector2 bottomCenter = new Vector2(center.x, center.y - extents.y);
            if (!_body.CastRay(bottomCenter, direction, distance, out var middleHit))
            {
                return false;
            }

            Vector2 bottomLeft = new Vector2(center.x - extents.x, center.y - extents.y);
            if (!_body.CastRay(bottomLeft, direction, middleHit.distance, out var leftHit))
            {
                return false;
            }

            Vector2 bottomRight = new Vector2(center.x + extents.x, center.y - extents.y);
            if (!_body.CastRay(bottomRight, direction, middleHit.distance, out var rightHit))
            {
                return false;
            }

            Debug.Log($"leftDist={leftHit.distance} middleDist={middleHit.distance} rightDist={rightHit.distance}");
            return middleHit.distance > leftHit.distance && middleHit.distance > rightHit.distance;
        }
    }
}

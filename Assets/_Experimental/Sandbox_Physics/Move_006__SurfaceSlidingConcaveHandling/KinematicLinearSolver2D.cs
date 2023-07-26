using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_006
{
    /*
    Collide and slide solver for movement.

    Notes
    - When moving along surfaces, we maintain a slight offset from the normal, such that contacts are
      intentionally avoided. This way, we avoid getting caught on edges and corners as easily      
    */
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;

        /* Number of iterations used to reach movement target before giving up. */
        private const int MaxMoveIterations = 10;

        /* Number of iterations used to reach no overlap before giving up. */
        private const int MaxOverlapIterations = 5;

        /* Amount which we consider to be (close enough to) zero. */
        private const float Epsilon = 0.005f;

        /* Amount used to ensure we don't get _too_ close to surfaces, to avoid getting stuck when moving tangential to a surface. */
        private const float ContactOffset = 0.05f;


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

        /*
        Note that with edge colliders, the collider will end up on either side, as there is no 'internal area'.

        This means that if our body starts in more overlapped position than separated from an edge collider, it will
        resolve to the 'inside' of the edge.
        
        In practice, this is not an issue except when spawning, as any movement in the solver caps changes in position be no
        greater than the body extents.
        */
        public void RemoveOverlap(Collider2D collider)
        {
            if (_body.IsFilteringLayerMask(collider.gameObject))
            {
                return;
            }
            
            // note that we remove separation if ever so slightly above surface as well
            Vector2 startPosition = _body.Position;
            int iteration = MaxOverlapIterations;
            ColliderDistance2D separation = _body.ComputeMinimumSeparation(collider);
            while (iteration-- > 0 && separation.distance < Epsilon)
            {
                Vector2 beforeStep = _body.Position;
                Debug.Log($"RemoveOverlap({collider.name}).substep#{MaxOverlapIterations - iteration} : " +
                          $"remaining={separation.distance}, direction={separation.normal}");
                _body.Position += separation.distance * separation.normal;

                Vector2 afterStep = _body.Position;
                Debug.DrawLine(beforeStep, afterStep, Color.yellow, 1f);
            }
            Vector2 endPosition = _body.Position;

            // bias the resolved position ever so slightly along the normal to prevent contact
            _body.Position += Epsilon * (endPosition - startPosition).normalized;
        }


        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        public void Move(Vector2 delta)
        {
            Debug.Log($"{(CheckForConcaveFaceBelow()?"yes":"no")}");

            // note that we compare extremely close to zero rather than our larger epsilon,
            // as delta can be very small depending on the physics step duration used to compute it
            if (delta == Vector2.zero)
            {
                return;
            }

            Vector2 startPosition = _body.Position;
            int iteration = MaxMoveIterations;
            float distanceRemaining = delta.magnitude;
            Vector2 direction = delta.normalized;
            while (iteration-- > 0 && distanceRemaining > Epsilon && direction.sqrMagnitude > Epsilon)
            {
                Vector2 beforeStep = _body.Position;
                Debug.DrawLine(beforeStep, beforeStep + (distanceRemaining * direction), Color.gray, 1f);
                
                Debug.Log($"Move({delta}).substep#{MaxMoveIterations-iteration} : " +
                          $"remaining={distanceRemaining}, direction={direction}");
                MoveUnobstructed(
                    direction,
                    distanceRemaining,
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
            Vector2 center  = _body.Center;
            Vector2 extents = _body.Extents;

            Vector2 bottomCenter = new Vector2(center.x, center.y - extents.y);
            if (!_body.CastRay(bottomCenter, Vector2.down, Mathf.Infinity, out var middleHit))
            {
                return false;
            }

            Vector2 bottomLeft = new Vector2(center.x - extents.x, center.y - extents.y);
            if (!_body.CastRay(bottomLeft, Vector2.down, Mathf.Infinity, out var leftHit))
            {
                return false;
            }

            Vector2 bottomRight = new Vector2(center.x + extents.x, center.y - extents.y);
            if (!_body.CastRay(bottomRight, Vector2.down, Mathf.Infinity, out var rightHit))
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

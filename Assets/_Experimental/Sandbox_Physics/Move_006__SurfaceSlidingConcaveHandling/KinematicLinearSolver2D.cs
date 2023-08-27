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

            if (_body.CastRayAt(collider, _body.Position, separation.normal, separation.distance, out var _))
            {
                // any surface passed through (tunneling) for the initial separation can be prevented by moving back along normal
                // May occur when body is heavily overlapped with an edge collider causing it 'snap' to other side
                Debug.Log($"RemoveOverlap({collider.name}).substep#precheck : Initial resolution caused collider to pass through an edge - pushing back to compensate");
                _body.IntersectAABB(_body.Position, separation.normal, out float distanceToAABBEdge);
                _body.Position += -2f * distanceToAABBEdge * separation.normal;
            }

            // note that we remove separation if ever so slightly above surface as well
            Vector2 startPosition = _body.Position;
            while (iteration-- > 0 && separation.distance is < -Epsilon or > Epsilon)
            {
                ColliderDistance2D previousSeparation = separation;
                Vector2 beforeStep = _body.Position;
                separation = _body.ComputeMinimumSeparation(collider);

                Debug.Log($"RemoveOverlap({collider.name}).substep#{MaxOverlapIterations - iteration} : remaining={separation.distance}, direction={separation.normal}");

                if (Mathf.Abs(separation.distance) > Mathf.Abs(previousSeparation.distance))
                {
                    // Any time separation not decreasing then bail out. May occur when moving into a protruding corner causing over-correction.
                    Debug.Log($"RemoveOverlap({collider.name}) : Separation amount increased from {previousSeparation.distance} to {separation.distance} - halting resolution");
                    break;
                }
                _body.Position += separation.distance * separation.normal;

                Vector2 afterStep = _body.Position;
                Debug.DrawLine(beforeStep, afterStep, Color.yellow, 1f);
            }
            Vector2 endPosition = _body.Position;

            // todo: investigate whether we should bias this even if the resolution didn't succeed
            // slightly bias the resolved position along normal to prevent contact
            // also prevents flip-flopping that can occur on subsequent calls when placed at center of an overlapped region
            _body.Position += ContactOffset * (endPosition - startPosition).normalized;
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
        public void Move(Vector2 direction, float distance)
        {
            direction.Normalize();

            // note that we compare extremely close to zero rather than our larger epsilon,
            // as delta can be very small depending on the physics step duration used to compute it
            if (distance * direction == Vector2.zero)
            {
                return;
            }

            float maxStep = ComputeMaxStep(direction, distance);

            // closest hit is sufficient for all cases except concave surfaces that will cause back and forth
            // movement due to 'flip-flopping' surface normals, so we if we detect one, treat it as a wall
            if (CheckForObstructingConcaveSurface(direction, maxStep, out float concaveDelta, out RaycastHit2D normalizedCenterHit) &&
                concaveDelta < ContactOffset)
            {
                Debug.Log($"Move({distance * direction}) : Obstructed by moving into a concave surface - halting movement");
                Debug.DrawLine(normalizedCenterHit.centroid, normalizedCenterHit.point, Color.blue, 1f);
                MoveToAvoidContact(normalizedCenterHit);
                return;
            }

            Vector2 startPosition = _body.Position;
            int iteration = MaxMoveIterations;
            while (iteration-- > 0 && distance > Epsilon && direction.sqrMagnitude > Epsilon)
            {
                Debug.Log($"Move({distance*direction}).substep#{MaxMoveIterations-iteration} : remaining={distance}, direction={direction}");
                MoveUnobstructed(direction, maxStep, out float step,out RaycastHit2D obstruction);

                direction -= obstruction.normal * Vector2.Dot(direction, obstruction.normal);
                distance -= step;
                maxStep = ComputeMaxStep(direction, distance);
            }
            Vector2 endPosition = _body.Position;
            _body.MovePositionWithoutBreakingInterpolation(startPosition, endPosition);
        }


        private void MoveUnobstructed(Vector2 direction, float maxStep, out float step, out RaycastHit2D obstruction)
        {
            if (_body.CastAABB(direction, maxStep, out var closestHit))
            {
                step = Mathf.Max(closestHit.distance - ContactOffset, 0f);
                obstruction = closestHit;
                Debug.DrawLine(_body.Position, _body.Position + step * direction, Color.cyan, 1f);
                DebugExtensions.DrawPlus(closestHit.point, new Vector2(ContactOffset, ContactOffset), 45f, Color.yellow, 1f);
            }
            else
            {
                step = maxStep;
                obstruction = default;
                Debug.DrawLine(_body.Position, _body.Position + step * direction, Color.magenta, 1f);
            }
            _body.Position += step * direction;
            MoveToAvoidContact(obstruction);
        }

        private bool CheckForObstructingConcaveSurface(Vector2 direction, float distance, out float delta, out RaycastHit2D normalizedHit)
        {
            delta = 0f;
            normalizedHit = default;

            int hitCount;
            ReadOnlySpan<RaycastHit2D> results;
            if (IsPerpendicularDirection(direction))
            {
                _body.CastRaysFromSide(direction, distance, rayCount: 3, out hitCount, out results);
            }
            else if (IsDiagonalDirection(direction))
            {
                _body.CastRaysFromCorner(spreadExtent: Epsilon, direction, distance, rayCount: 3, out hitCount, out results);
            }
            else
            {
                return false;
            }

            // technically it is possible that the collider between left/right/middle along a
            // body's edge is different, but we're not going to worry about that case
            RaycastHit2D leftHit   = results[0];
            RaycastHit2D middleHit = results[1];
            RaycastHit2D rightHit  = results[2];

            if (!leftHit || !rightHit)
            {
                return false;
            }
            if (middleHit && (middleHit.distance <= leftHit.distance || middleHit.distance <= rightHit.distance))
            {
                return false;
            }

            Vector2 midPoint = Vector2.LerpUnclamped(leftHit.centroid, rightHit.centroid, 0.50f);

            // construct a hit equivalent to moving towards a flat wall spanning between the left and right hits
            delta = Mathf.Abs(leftHit.distance - rightHit.distance);
            normalizedHit          = leftHit.distance < rightHit.distance ? leftHit : rightHit;
            normalizedHit.centroid = midPoint;
            normalizedHit.point    = midPoint + normalizedHit.distance * direction;
            normalizedHit.normal   = -direction;
            return true;
        }


        private float ComputeMaxStep(Vector2 direction, float distance)
        {
            _body.IntersectAABB(_body.Position, direction, out float bodyRadius);
            return Mathf.Min(distance, bodyRadius);
        }

        private bool IsDiagonalDirection(Vector2 direction)
        {
            bool areComponentsEqual = Mathf.Approximately(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            return areComponentsEqual;
        }

        private bool IsPerpendicularDirection(Vector2 direction)
        {
            bool isXZero = Mathf.Approximately(direction.x, 0);
            bool isYZero = Mathf.Approximately(direction.y, 0);
            return (isXZero && !isYZero) || (!isXZero && isYZero);
        }

        private void MoveToAvoidContact(RaycastHit2D hit)
        {
            float adjustmentAmount = Mathf.Max(0f, ContactOffset - hit.distance);
            if (adjustmentAmount > 0f)
            {
                _body.Position += adjustmentAmount * hit.normal;
            }
        }
    }
}

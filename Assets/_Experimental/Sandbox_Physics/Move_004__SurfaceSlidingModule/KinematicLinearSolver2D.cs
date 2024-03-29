using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_004
{
    internal sealed class KinematicLinearSolver2D
    {
        private KinematicBody2D _body;

        /* Number of iterations used to reach movement target before giving up. */
        private const int MaxIterations = 10;

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
        public bool RemoveOverlap(Collider2D collider)
        {
            if (_body.IsFilteringLayerMask(collider.gameObject))
            {
                return false;
            }

            ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(collider);

            bool overlapped = minimumSeparation.isOverlapped;
            Vector2 offset = minimumSeparation.distance * minimumSeparation.normal;

            Debug.Log($"RemoveOverlap({collider.name}) : overlapped={overlapped} offset={offset}");
            Debug.DrawLine(_body.Position, _body.Position + offset, overlapped ? Color.green : Color.red, 1f);

            if (!overlapped)
            {
                return false;
            }

            if (minimumSeparation.distance < Epsilon)
            {
                _body.Position += offset;
            }
            return true;
        }

        
        /* Move using the new rigidbody slide helper provided in the Unity 2023 tech stream. */
        public void MoveUsingNewUnitySlideModule(float time, Vector2 velocity, Rigidbody2D.SlideMovement slideParams)
        {
            if (time <= 0f || velocity == Vector2.zero)
            {
                return;
            }

            Rigidbody2D.SlideResults results = _body.Slide(time, velocity, slideParams);

            Debug.Log($"move : method=NewUnitySlideModule iterationsUsed={results.iterationsUsed}");
        }


        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        public void MoveUsingJeffAlgo(Vector2 delta)
        {
            Vector2 startPosition = _body.Position;

            int iteration = MaxIterations;
            float distanceRemaining = delta.magnitude;
            Vector2 direction = delta.normalized;
            while (iteration-- > 0 && distanceRemaining > Epsilon && direction.sqrMagnitude > Epsilon)
            {
                MoveUnobstructed(
                    distanceRemaining,
                    direction,
                    out float step,
                    out RaycastHit2D obstruction);

                direction -= obstruction.normal * Vector2.Dot(direction, obstruction.normal);
                distanceRemaining -= step;
            }

            Vector2 endPosition = _body.Position;

            _body.MovePositionWithoutBreakingInterpolation(startPosition, endPosition);

            Debug.Log($"move : method=jeffAlgo iterationsUsed={MaxIterations - iteration}");
        }

        /* Project body along delta until (if any) obstruction. Distance swept is capped at body-radius to prevent tunneling. */
        private void MoveUnobstructed(float distance, Vector2 direction, out float step, out RaycastHit2D obstruction)
        {
            // slightly bias the start position such that box casts still resolve
            // even when AABB is touching a collider in that 
            float bodyRadius = _body.ComputeDistanceToEdge(direction);
            _body.Position -= Epsilon * direction;

            step = distance < bodyRadius ? distance : bodyRadius;
            if (_body.CastAABB(direction, step + Epsilon, out obstruction))
            {
                step = obstruction.distance - ContactOffset;
            }
            _body.Position += (step + Epsilon) * direction;
        }
    }
}

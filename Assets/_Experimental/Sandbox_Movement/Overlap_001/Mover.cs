using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Overlap_001
{
    public sealed class Mover : MonoBehaviour
    {
        private Body _body;

        private float _previousDepenetration;
        private ColliderDistance2D _previousMinSeparation;
        private (Vector2 from, Vector2 to) _previousMove;

        [Pure]
        private (float distance, Vector2 direction) DecomposeDelta(Vector2 delta)
        {
            // below is exactly equivalent to (delta.normalized, delta.magnitude) without the redundant
            // sqrt call (benchmarks showed ~54% faster), and without NaNs on zero length vectors by just dividing distance
            float squaredMagnitude = delta.sqrMagnitude;
            if (squaredMagnitude <= 1E-010f)
            {
                return (0f, Vector2.zero);
            }

            float magnitude = Mathf.Sqrt(squaredMagnitude);
            delta /= magnitude;
            return (magnitude, delta);
        }


        void Awake()
        {
            _body = transform.GetComponent<Body>();
            _previousMinSeparation = default;
            _previousMove = (Vector2.zero, Vector2.zero);
        }

        public void MoveTo(Vector2 target)
        {
            _previousMove = (_body.Position, target);
            _body.MoveBy(_previousMove.to - _previousMove.from);

            Vector2 position = _body.Position;
            ResolveDepenetrationAlongLastMove();
            _body.MoveTo(position);
        }

        public void ResolveDepenetrationAlongLastMove()
        {
            RaycastHit2D obstruction = default;
            (float step, Vector2 direction) = DecomposeDelta(_previousMove.to - _previousMove.from);
            if (_body.CastAABB(direction, step, out ReadOnlySpan<RaycastHit2D> hits, false))
            {
                obstruction = hits[0];
                step = hits[0].distance;
            }
            _body.MoveBy(step * direction);

            // if there was an obstruction, apply any depenetration
            if (obstruction && ComputeDepenetration(obstruction.collider, direction, step, out float separation) && separation < 0)
            {
                _body.MoveBy(separation * direction);
            }
        }
        

        /*
        Compute signed distance representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for complex polygons.
        */
        private bool ComputeDepenetration(Collider2D collider, Vector2 direction, float maxScanDistance, out float separation)
        {
            separation = 0f;
            _previousDepenetration = separation;

            ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(collider);
            _previousMinSeparation = minimumSeparation;
            if (minimumSeparation.distance * minimumSeparation.normal == Vector2.zero)
            {
                return false;
            }

            Vector2 pointOnAABBEdge = minimumSeparation.pointA;
            Vector2 directionToSurface = minimumSeparation.isOverlapped ? -direction : direction;
            if (_body.CastRayAt(collider, pointOnAABBEdge, directionToSurface, maxScanDistance, out RaycastHit2D hit, false))
            {
                separation = minimumSeparation.isOverlapped ? -Mathf.Abs(hit.distance) : Mathf.Abs(hit.distance);
                Debug.Log(separation);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Given {collider} not found between " +
                    $"{pointOnAABBEdge} and {pointOnAABBEdge + maxScanDistance * direction}");
            }

            _previousDepenetration = separation;
            return (separation * direction) != Vector2.zero;
        }
        
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            GizmoExtensions.DrawArrow(_previousMove.from, _previousMove.to, Color.white);

            GizmoExtensions.DrawSphere(_previousMinSeparation.pointA, 0.05f, Color.blue);
            GizmoExtensions.DrawSphere(_previousMinSeparation.pointB, 0.05f, Color.cyan);
            GizmoExtensions.DrawArrow(_previousMove.to, _previousMove.to - _previousDepenetration * (_previousMove.to - _previousMove.from).normalized, Color.black);

        }
        #endif
    }
}

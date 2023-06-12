using System;
using UnityEngine;


namespace PQ._Experimental.Overlap_001
{
    public sealed class Mover : MonoBehaviour
    {
        private Body _body;

        private ColliderDistance2D _minSep;
        private (Vector2 from, Vector2 to) _previousMove;
        private (Vector2 before, Vector2 after) _previousDepenetrate;


        void Awake()
        {
            _body = transform.GetComponent<Body>();
            _minSep = default;
            _previousMove = (_body.Position, _body.Position);
            _previousDepenetrate = (_body.Position, _body.Position);
        }

        public void MoveTo(Vector2 target)
        {
            _previousMove = (_body.Position, target);
            _body.MoveBy(_previousMove.to - _previousMove.from);
        }

        // if direction was zero, then resolve to min separation
        public void DepenetrateAlongLastMove()
        {
            Vector2 direction = (_previousMove.to - _previousMove.from).normalized;
            float maxDistance = _body.ComputeDistanceToEdge(direction);

            RaycastHit2D obstruction = default;
            if (_body.CastAABB(direction, maxDistance, out ReadOnlySpan<RaycastHit2D> hits, true))
            {
                obstruction = hits[0];
            }

            // if there was an obstruction, apply any depenetration
            _previousDepenetrate = (_body.Position, _body.Position);
            if (obstruction && ComputeDepenetration(obstruction.collider, direction, maxDistance, out float separation) && separation < 0)
            {
                _previousDepenetrate = (_body.Position, _body.Position + separation * direction);
                _body.MoveTo(_previousDepenetrate.after);
            }
        }
        

        /*
        Compute signed distance representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for complex polygons.
        */
        private bool ComputeDepenetration(Collider2D collider, Vector2 direction, float maxScanDistance, out float separation)
        {
            separation = 0f;

            _minSep = _body.ComputeMinimumSeparation(collider);
            if (_minSep.distance * _minSep.normal == Vector2.zero)
            {
                return false;
            }
            if (direction == Vector2.zero)
            {
                separation = _minSep.isOverlapped ? -Mathf.Abs(_minSep.distance) : Mathf.Abs(_minSep.distance);
                return (separation * direction) != Vector2.zero;
            }

            Vector2 pointOnAABBEdge = _minSep.pointA;
            Vector2 directionToSurface = _minSep.isOverlapped ? -direction : direction;
            if (_body.CastRayAt(collider, pointOnAABBEdge, directionToSurface, maxScanDistance, out RaycastHit2D hit, false))
            {
                separation = _minSep.isOverlapped ? -Mathf.Abs(hit.distance) : Mathf.Abs(hit.distance);
                Debug.Log(separation);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Given {collider} not found between " +
                    $"{pointOnAABBEdge} and {pointOnAABBEdge + maxScanDistance * direction}");
            }

            return (separation * direction) != Vector2.zero;
        }
        
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            GizmoExtensions.DrawArrow(_previousMove.from, _previousMove.to, Color.white);
            GizmoExtensions.DrawSphere(_minSep.pointA, 0.01f, Color.blue);
            GizmoExtensions.DrawSphere(_minSep.pointB, 0.01f, Color.cyan);
            GizmoExtensions.DrawArrow(_previousDepenetrate.before, _previousDepenetrate.after, Color.green);
        }
        #endif
    }
}

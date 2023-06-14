using System;
using UnityEngine;


namespace PQ._Experimental.Overlap_002
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

        // note - resolves to min separation if the last move distance was zero
        public void DepenetrateAlongLastMove()
        {
            Vector2 position  = _body.Position;
            Vector2 direction = (_previousMove.to - _previousMove.from).normalized;
            float maxDistance = _body.ComputeDistanceToEdge(direction);

            RaycastHit2D obstruction = default;
            if (_body.CastAABB(direction, maxDistance, out ReadOnlySpan<RaycastHit2D> hits, true))
            {
                obstruction = hits[0];
            }

            // if there was an obstruction, apply any depenetration
            _previousDepenetrate = (position, position);
            if (obstruction && ComputeDepenetration(obstruction.collider, direction, 2f * _body.Extents.magnitude, out float separation, out Vector2 resolveNormal))
            {
                _body.MoveBy(separation * resolveNormal);
                _previousDepenetrate = (position, position + separation * resolveNormal);
            }
        }
        

        /*
        Compute signed distance representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for complex polygons.
        */
        private bool ComputeDepenetration(Collider2D collider, Vector2 direction, float maxDepenetrateAmount, out float separation, out Vector2 resolveNormal)
        {
            separation = 0f;
            resolveNormal = direction;

            _minSep = _body.ComputeMinimumSeparation(collider);
            if (_minSep.distance * _minSep.normal == Vector2.zero)
            {
                return false;
            }
            if (direction == Vector2.zero)
            {
                separation = _minSep.isOverlapped ? -Mathf.Abs(_minSep.distance) : Mathf.Abs(_minSep.distance);
                resolveNormal = _minSep.normal;
                return (separation * resolveNormal) != Vector2.zero;
            }

            Vector2 pointOnAABBEdge = _minSep.pointA;
            Vector2 directionToSurface = _minSep.isOverlapped ? -direction : direction;

            Debug.DrawLine(pointOnAABBEdge, pointOnAABBEdge + maxDepenetrateAmount * directionToSurface, Color.red, 2f);
            if (_body.CastRayAt(collider, pointOnAABBEdge, directionToSurface, maxDepenetrateAmount, out RaycastHit2D hit, false))
            {
                separation = _minSep.isOverlapped ? -Mathf.Abs(hit.distance) : Mathf.Abs(hit.distance);
                Debug.DrawLine(pointOnAABBEdge, pointOnAABBEdge + separation * directionToSurface, Color.green, 2f);
            }
            else
            {
                throw new InvalidOperationException( 
                    $"Given {collider} not found between " +
                    $"{pointOnAABBEdge} and {pointOnAABBEdge + maxDepenetrateAmount * direction}");
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

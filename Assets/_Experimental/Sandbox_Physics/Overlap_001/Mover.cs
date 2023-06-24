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
            if (obstruction && ComputeSeparation(obstruction.collider, direction, out float separation, out Vector2 resolveNormal, out _))
            {
                _body.MoveBy(separation * resolveNormal);
                _previousDepenetrate = (position, position + separation * resolveNormal);
            }
        }
        
        /*
        Compute vector needed to resolve separation amount between AABB and collider along axis, if any.

        If colliders are overlapping with no axis specified (ie zero), then depenetrates along minimum separation.
        Note that exceptions are thrown if colliders are in invalid states (ie null/disabled, or not found along direction).
        Note that since separating axis theorem is used, many invocations may be needed for complex polygons.
        */
        private bool ComputeSeparation(Collider2D collider, Vector2 axis, out float separation, out Vector2 direction, out bool overlapped)
        {
            // ensure it's possible to get a valid minimum separation (ie both non-null and enabled)
            ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(collider);
            if (!minimumSeparation.isValid)
            {
                throw new InvalidOperationException($"Invalid minimum separation distance between body and collider={collider}");
            }

            // use our minimum separation if axis not specified or no overlap
            Vector2 minimumOffset = minimumSeparation.distance * minimumSeparation.normal;
            if (axis == Vector2.zero || !minimumSeparation.isOverlapped)
            {
                separation = minimumSeparation.distance;
                direction  = minimumSeparation.normal;
                overlapped = minimumSeparation.isOverlapped;
                return minimumOffset != Vector2.zero;
            }

            // todo: figure out a reasonable way to limit the raycast distance here without it being explicitly passed in
            // todo: verify if this still works with edge radius and casted from inside (if not, might need to use closest point or similar)
            // todo: using pointA as our origin only valid for some overlap cases, special casing depenetration checks
            //       to perform min separation twice should account for cases of two or three overlapping AABB corners
            direction = -axis;
            Vector2 pointOnAABBEdge = minimumSeparation.pointA;
            if (!_body.CastRayAt(collider, pointOnAABBEdge, -direction, Mathf.Infinity, out RaycastHit2D hit, false))
            {
                throw new InvalidOperationException($"Collider={collider} not found along direction={direction}");
            }

            // todo: make sure the distance isn't negative even if starting inside collider (like how bounds.intersect does it)
            separation = hit.distance;
            direction  = -axis;
            overlapped = true;
            return true;
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

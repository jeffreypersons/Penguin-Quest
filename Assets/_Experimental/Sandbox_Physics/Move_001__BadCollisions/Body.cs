using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_001
{
    public class Body : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   _rigidbody;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _layerMask = default;
        [SerializeField] [Range(0,   1)] private float _skinWidth = 0.075f;
        [SerializeField] [Range(1, 100)] private int _preallocatedBufferSize = 16;

        private ContactFilter2D _contactFilter;
        private RaycastHit2D[]  _hitBuffer;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position  => _rigidbody.position;
        public float   Depth     => _rigidbody.transform.position.z;
        public Bounds  Bounds    => _boxCollider.bounds;
        public Vector2 Forward   => _rigidbody.transform.right.normalized;
        public Vector2 Up        => _rigidbody.transform.up.normalized;
        public float   SkinWidth => _skinWidth;


        void Awake()
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var _))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<BoxCollider2D>(out var _))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {transform}");
            }

            _contactFilter = new ContactFilter2D();
            _hitBuffer     = new RaycastHit2D[_preallocatedBufferSize];

            _boxCollider.edgeRadius = _skinWidth;
            _boxCollider.size = new Vector2(1f - _skinWidth, 1f - _skinWidth);

            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody.isKinematic = true;
            _rigidbody.simulated   = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(_layerMask);
        }


        /* Immediately set facing of horizontal/vertical axes. */
        public void Flip(bool horizontal, bool vertical)
        {
            _rigidbody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.transform.localEulerAngles = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);
            _rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        /* Immediately move body by given amount. */
        public void MoveBy(Vector2 delta)
        {
            _rigidbody.position += delta;
        }

        /* Interpolated move this amount. */
        public void MovePosition(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
        {
            _rigidbody.position = startPositionThisFrame;
            _rigidbody.MovePosition(targetPositionThisFrame);
        }
        
        /*
        Project point along given direction and local offset from AABB center, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastRay(Vector2 centerOffset, Vector2 direction, float distance, out ReadOnlySpan<RaycastHit2D> hits, bool includeAlreadyOverlappingColliders)
        {
            Vector2 origin = (Vector2)_boxCollider.bounds.center + centerOffset;

            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;
            _boxCollider.enabled = false;

            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);
            hits = _hitBuffer.AsSpan(0, hitCount);

            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;
            _boxCollider.enabled = true;

            #if UNITY_EDITOR
            float duration = Time.fixedDeltaTime;
            Vector2 edgePoint = hits[0].point - (hits[0].distance * direction);
            Vector2 hitPoint  = hits[0].point;
            Vector2 endPoint  = hits[0].point + (distance * direction);

            Debug.DrawLine(edgePoint, hitPoint, Color.green, duration);
            Debug.DrawLine(hitPoint,  endPoint, Color.red,   duration);
            #endif
            return !hits.IsEmpty;
        }


        /*
        Project a point along given direction until specific given collider is hit.

        Note that in 3D we have collider.RayCast for this, but in 2D we have no built in way of checking a
        specific collider (collider2D.RayCast confusingly casts _from_ it instead of _at_ it).
        */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
        {
            int layer = collider.gameObject.layer;
            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            LayerMask includeLayers = _contactFilter.layerMask;
            collider.gameObject.layer = Physics2D.IgnoreRaycastLayer;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;
            _contactFilter.SetLayerMask(~collider.gameObject.layer);

            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);

            collider.gameObject.layer = layer;
            _contactFilter.SetLayerMask(includeLayers);
            Physics2D.queriesStartInColliders = queriesStartInColliders;

            hit = default;
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider == collider)
                {
                    hit = _hitBuffer[i];
                    break;
                }
            }
            #if UNITY_EDITOR
            float duration = Time.fixedDeltaTime;
            Vector2 edgePoint = hit.point - (hit.distance * direction);
            Vector2 hitPoint = hit.point;
            Vector2 endPoint = hit.point + (distance * direction);

            Debug.DrawLine(edgePoint, hitPoint, Color.green, duration);
            Debug.DrawLine(hitPoint,  endPoint, Color.red,   duration);
            #endif
            return hit;
        }


        /* Project AABB along delta, and return ALL hits (if any). */
        public bool CastAABB(Vector2 direction, float distance, out ReadOnlySpan<RaycastHit2D> hits, bool includeAlreadyOverlappingColliders)
        {
            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;

            int hitCount = _boxCollider.Cast(direction, _contactFilter, _hitBuffer, distance, ignoreSiblingColliders: true);
            hits = _hitBuffer.AsSpan(0, hitCount);

            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;

            #if UNITY_EDITOR
            float duration = Time.fixedDeltaTime;
            foreach (RaycastHit2D hit in hits)
            {
                Vector2 edgePoint = hit.point - (hit.distance * direction);
                Vector2 hitPoint = hit.point;
                Vector2 endPoint = hit.point + (distance * direction);

                Debug.DrawLine(edgePoint, hitPoint, Color.green, duration);
                Debug.DrawLine(hitPoint,  endPoint, Color.red,   duration);
            }
            #endif
            return !hits.IsEmpty;
        }
        
        
        /*
        Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width.

        If no layermask provided, uses the one assigned in editor.
        */
        public CollisionFlags2D CheckSides()
        {
            _contactFilter.useNormalAngle = true;
            bool isFlippedHorizontal = _rigidbody.transform.localEulerAngles.y >= 90f;
            bool isFlippedVertical   = _rigidbody.transform.localEulerAngles.x >= 90f;

            CollisionFlags2D flags = CollisionFlags2D.None;
            if (HasContactsInNormalRange(315, 45))
            {
                flags |= isFlippedHorizontal? CollisionFlags2D.Behind : CollisionFlags2D.Front;
            }
            if (HasContactsInNormalRange(45, 135))
            {
                flags |= isFlippedVertical ? CollisionFlags2D.Above : CollisionFlags2D.Below;
            }
            if (HasContactsInNormalRange(135, 225))
            {
                flags |= isFlippedHorizontal ? CollisionFlags2D.Front : CollisionFlags2D.Behind;
            }
            if (HasContactsInNormalRange(225, 315))
            {
                flags |= isFlippedVertical ? CollisionFlags2D.Below : CollisionFlags2D.Above;
            }
            _contactFilter.useNormalAngle = false;
            Debug.Log(flags);
            return flags;
        }

        
        /* Compute distance from center to edge of our bounding box in given direction. */
        public float ComputeDistanceToEdge(Vector2 direction)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.IntersectRay(new Ray(bounds.center, direction), out float distanceFromCenterToEdge);

            // discard sign since distance is negative if starts within bounds (contrary to other ray methods)
            return Mathf.Abs(distanceFromCenterToEdge);
        }

        /*
        Compute vector needed to resolve separation amount between AABB and collider along axis, if any.

        If colliders are overlapping with no axis specified (ie zero), then depenetrates along minimum separation.
        Note that exceptions are thrown if colliders are in invalid states (ie null/disabled, or not found along direction).
        Note that since separating axis theorem is used, many invocations may be needed for complex polygons.
        */
        public bool ComputeSeparation(Collider2D collider, Vector2 axis, out float separation, out Vector2 direction, out bool overlapped)
        {
            // ensure it's possible to get a valid minimum separation (ie both non-null and enabled)
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
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
            if (!CastRayAt(collider, pointOnAABBEdge, -direction, Mathf.Infinity, out RaycastHit2D hit, false))
            {
                throw new InvalidOperationException($"Collider={collider} not found along direction={direction}");
            }

            // todo: make sure the distance isn't negative even if starting inside collider (like how bounds.intersect does it)
            separation = hit.distance;
            direction  = -axis;
            overlapped = true;
            return true;
        }
        
        /*
        Compute vector needed to resolve separation amount between AABB and collider along axis, if any.

        If colliders are overlapping with no axis specified (ie zero), then depenetrates along minimum separation.
        Note that exceptions are thrown if colliders are in invalid states (ie null/disabled, or not found along direction).
        Note that since separating axis theorem is used, many invocations may be needed for complex polygons.
        */
        public bool ComputeSeparation2(Collider2D collider, Vector2 axis, out float separation, out Vector2 direction, out bool overlapped)
        {
            // ensure it's possible to get a valid minimum separation (ie both non-null and enabled)
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
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
            if (!CastRayAt(collider, pointOnAABBEdge, -direction, Mathf.Infinity, out RaycastHit2D hit, false))
            {
                throw new InvalidOperationException($"Collider={collider} not found along direction={direction}");
            }

            // todo: make sure the distance isn't negative even if starting inside collider (like how bounds.intersect does it)
            separation = hit.distance;
            direction  = -axis;
            overlapped = true;
            return true;
        }

        
        private bool HasContactsInNormalRange(float min, float max)
        {
            float previousMin = _contactFilter.minNormalAngle;
            float previousMax = _contactFilter.maxNormalAngle;

            _contactFilter.SetNormalAngle(min, max);
            bool hasContactsInRange = _boxCollider.IsTouching(_contactFilter);

            _contactFilter.SetNormalAngle(previousMin, previousMax);
            return hasContactsInRange;
        }
        
        #if UNITY_EDITOR
        void OnValidate()
        {
            _boxCollider.edgeRadius = _skinWidth;
            _boxCollider.size = new Vector2(1f - _skinWidth, 1f - _skinWidth);
            if (!Application.IsPlaying(this))
            {
                return;
            }

            if (_hitBuffer == null || _preallocatedBufferSize != _hitBuffer.Length)
            {
                _hitBuffer = new RaycastHit2D[_preallocatedBufferSize];
            }
        }

        void OnDrawGizmos()
        {
            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // surrounded by an outer bounding box offset by our skin with, with a pair of arrows from the that
            // should be identical to the transform's axes in the editor window
            Bounds box = Bounds;
            Vector2 center    = new(box.center.x, box.center.y);
            Vector2 skinRatio = new(1f + (_skinWidth / box.extents.x), 1f + (_skinWidth / box.extents.y));
            Vector2 xAxis     = box.extents.x * Forward;
            Vector2 yAxis     = box.extents.y * Up;

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.black);
            GizmoExtensions.DrawRect(center, skinRatio.x * xAxis, skinRatio.y * yAxis, Color.black);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }
        #endif
    }
}

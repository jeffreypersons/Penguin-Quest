using System;
using UnityEngine;


namespace PQ._Experimental.Overlap_001
{
    public class Body : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   _rigidbody;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _layerMask = default;
        [SerializeField] [Range(0,   1)] private float _skinWidth = 0.075f;
        [SerializeField] [Range(1, 100)] private int _preallocatedHitBufferSize = 16;

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
            _hitBuffer     = new RaycastHit2D[_preallocatedHitBufferSize];

            _boxCollider.edgeRadius = _skinWidth;
            _boxCollider.size = new Vector2(1f - _skinWidth, 1f - _skinWidth);

            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody.isKinematic = true;
            _rigidbody.simulated   = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(_layerMask);
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
        
        
        /* Compute distance from center to edge of our bounding box in given direction. */
        public float ComputeDistanceToEdge(Vector2 direction)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.IntersectRay(new Ray(bounds.center, direction), out float distanceFromCenterToEdge);

            // discard sign since distance is negative if starts within bounds (contrary to other ray methods)
            return Mathf.Abs(distanceFromCenterToEdge);
        }
        
        /*
        Compute signed distance representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for complex polygons.
        */
        public bool ComputeDepenetration(Collider2D collider, Vector2 direction, float maxScanDistance, out float separation)
        {
            separation = 0f;

            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (minimumSeparation.distance * minimumSeparation.normal == Vector2.zero)
            {
                return false;
            }

            Vector2 pointOnAABBEdge = minimumSeparation.pointA;
            Vector2 directionToSurface = minimumSeparation.isOverlapped ? -direction : direction;
            if (CastRayAt(collider, pointOnAABBEdge, directionToSurface, maxScanDistance, out RaycastHit2D hit, false))
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

            return (separation * direction) != Vector2.zero;
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

        /*
        Project a point along given direction until specific given collider is hit.
        */
        private bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
        {
            // note that in 3D we have collider.RayCast for this, but in 2D we have no built in way of
            // checking a specific collider (collider2D.RayCast confusingly casts _from_ it instead of _at_ it)
            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;
            LayerMask previousLayerMask = _contactFilter.layerMask;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;
            _contactFilter.SetLayerMask(~collider.gameObject.layer);
            _boxCollider.enabled = false;

            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);

            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;
            _contactFilter.SetLayerMask(previousLayerMask);
            _boxCollider.enabled = true;

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


        #if UNITY_EDITOR
        void OnValidate()
        {
            _boxCollider.edgeRadius = _skinWidth;
            _boxCollider.size = new Vector2(1f - _skinWidth, 1f - _skinWidth);
            if (!Application.IsPlaying(this))
            {
                return;
            }

            if (_hitBuffer == null || _preallocatedHitBufferSize != _hitBuffer.Length)
            {
                _hitBuffer = new RaycastHit2D[_preallocatedHitBufferSize];
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

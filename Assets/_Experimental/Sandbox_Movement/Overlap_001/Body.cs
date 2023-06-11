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


        public bool CastAABB(Vector2 direction, float distance, out ReadOnlySpan<RaycastHit2D> hits, bool includeAlreadyOverlappingColliders)
        {
            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;

            int hitCount = _boxCollider.Cast(direction, _contactFilter, _hitBuffer, distance, ignoreSiblingColliders: true);
            hits = _hitBuffer.AsSpan(0, hitCount);

            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;

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
        
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
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
            return hit;
        }
        
        /*
        Compute vector representing overlap amount between body and given collider, if any.

        Note that uses separating axis theorem to determine overlap, so may require more invocations to resolve overlap
        for complex collider shapes (eg convex polygons).
        */
        public ColliderDistance2D ComputeMinimumSeparation(Collider2D collider)
        {
            if (collider == null)
            {
                throw new ArgumentNullException("Error state - invalid minimum separation between body and given collider");
            }

            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (!minimumSeparation.isValid)
            {
                throw new InvalidOperationException("Error state - invalid minimum separation between body and given collider");
            }
            return minimumSeparation;
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

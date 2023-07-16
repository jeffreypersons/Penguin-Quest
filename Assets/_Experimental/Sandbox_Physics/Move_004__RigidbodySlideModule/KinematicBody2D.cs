using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_004
{
    internal sealed class KinematicBody2D
    {
        private Transform        _transform;
        private Rigidbody2D      _rigidbody;
        private BoxCollider2D    _boxCollider;
        private ContactFilter2D  _contactFilter;
        private RaycastHit2D[]   _hitBuffer;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;

        private const int DefaultBufferSize = 16;

        public override string ToString() =>
            $"{GetType()}{{" +
                $"Position:{Position}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB:bounds(center:{Center},extents:{Extents})," +
                $"SkinWidth:{SkinWidth}," +
                $"LayerMask:{LayerMask}," +
            $"}}";

        public LayerMask LayerMask => _contactFilter.layerMask;

        public Vector2 Position
        {
            get => _rigidbody.position;
            set => _rigidbody.position = value;
        }

        public Vector3 Rotation
        {
            get => _transform.localEulerAngles;
            set => _transform.localEulerAngles = value;
        }

        public Vector2 Center       => _boxCollider.bounds.center;
        public Vector2 Forward      => _transform.right.normalized;
        public Vector2 Up           => _transform.up.normalized;
        public Vector2 Extents      => _boxCollider.bounds.extents + new Vector3(_boxCollider.edgeRadius, _boxCollider.edgeRadius, 0f);
        public float   Depth        => _transform.position.z;
        public float   SkinWidth    => _boxCollider.edgeRadius;
        public Vector2 BoundsOffset => _boxCollider.offset;

        
        #if UNITY_EDITOR
        public bool DrawShapeCastsInEditor { get; set; } = true;
        public bool DrawRayCastsInEditor   { get; set; } = true;
        #endif

        public KinematicBody2D(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!transform.TryGetComponent(out Rigidbody2D rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(transform)}");
            }
            if (!transform.TryGetComponent(out BoxCollider2D boxCollider2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(BoxCollider2D)} - not found on {nameof(transform)}");
            }
            if (!ReferenceEquals(boxCollider2D.attachedRigidbody, rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(boxCollider2D)}");
            }

            _transform     = rigidbody2D.transform;
            _rigidbody     = rigidbody2D;
            _boxCollider   = boxCollider2D;
            _contactFilter = new ContactFilter2D();
            _hitBuffer     = new RaycastHit2D[DefaultBufferSize];
            _overlapBuffer = new Collider2D[DefaultBufferSize];
            _contactBuffer = new ContactPoint2D[DefaultBufferSize];

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;
        }

        public bool IsAttachedTo(Transform transform)
        {
            return ReferenceEquals(_transform, transform);
        }

        /*
        Reset to position at start of frame and apply MovePosition. This preserves interpolation despite any changes to position.
        
        This works around the fact that modifying rigidbody.position prevents unity from applying smoothing based on previous
        frame positions. By 'clearing' any manual changes to position and invoking movePosition, we can maintain interpolation.
        */
        public void MovePositionWithoutBreakingInterpolation(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
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
            if (DrawRayCastsInEditor)
            {
                float duration = Time.fixedDeltaTime;
                DebugExtensions.DrawRayCast(origin, direction, distance, hits.IsEmpty? default : hits[0], duration);
            }
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
            if (DrawRayCastsInEditor)
            {
                DebugExtensions.DrawRayCast(origin, direction, distance, hit, Time.fixedDeltaTime);
            }
            #endif
            return hit;
        }

        /*
        Project AABB along given delta from AABB center, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastAABB(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            Physics2D.queriesStartInColliders = false;
            if (_boxCollider.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }
            Physics2D.queriesStartInColliders = false;
            return hit;
        }

        /*
        Compute distance from center to edge of our bounding box in given direction.
        */
        public float ComputeDistanceToEdge(Vector2 direction)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.IntersectRay(new Ray(bounds.center, direction), out float distanceFromCenterToEdge);

            // discard sign since distance is negative if starts within bounds (contrary to other ray methods)
            return Mathf.Abs(distanceFromCenterToEdge);
        }
    }
}

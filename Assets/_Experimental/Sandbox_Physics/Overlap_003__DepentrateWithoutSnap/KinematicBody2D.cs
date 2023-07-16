using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Overlap_003
{
    internal sealed class KinematicBody2D
    {
        private const int DefaultBufferSize = 16;

        private Rigidbody2D      _rigidbody;
        private CircleCollider2D _circleCollider;
        private ContactFilter2D  _contactFilter;
        private RaycastHit2D[]   _hitBuffer;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;


        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"Radius: {Radius}," +
            $"}}";

        public Vector2 Position => _rigidbody.position;
        public float   Radius   => _circleCollider.radius;
        public Vector2 Forward  => _rigidbody.transform.localEulerAngles.y >= 90f ? Vector2.left : Vector2.right;
        public Vector2 Up       => _rigidbody.transform.localEulerAngles.x >= 90f ? Vector2.down : Vector2.up;


        public KinematicBody2D(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(Transform)}");
            }
            if (!transform.TryGetComponent<Rigidbody2D>(out var rigidbody))
            {
                throw new MissingComponentException($"Expected attached Rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<CircleCollider2D>(out var circleCollider))
            {
                throw new MissingComponentException($"Expected attached CircleCollider2D - not found on {transform}");
            }

            _rigidbody      = rigidbody;
            _circleCollider = circleCollider;

            _contactFilter = new ContactFilter2D();
            _hitBuffer     = new RaycastHit2D[DefaultBufferSize];
            _overlapBuffer = new Collider2D[DefaultBufferSize];
            _contactBuffer = new ContactPoint2D[DefaultBufferSize];

            _rigidbody.isKinematic = true;
            _rigidbody.simulated   = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));
        }


        public void MoveTo(Vector2 position) => _rigidbody.position = position;
        public void MoveBy(Vector2 delta) => _rigidbody.position += delta;
        public bool IsTouching(Collider2D collider) => _circleCollider.IsTouching(collider);

        /* Check for overlapping colliders within our bounding box. */
        public bool CheckForOverlappingColliders(out ReadOnlySpan<Collider2D> colliders)
        {
            int colliderCount = _circleCollider.Overlap(_contactFilter, _overlapBuffer);
            colliders = _overlapBuffer.AsSpan(0, colliderCount);
            return !colliders.IsEmpty;
        }

        /* Query existing contacts from last physics pass. If in middle of fixedUpdate after modifying position, will need to syncTransforms. */
        public bool CheckForContacts(out ReadOnlySpan<ContactPoint2D> contacts)
        {
            int contactCount = _circleCollider.GetContacts(_contactFilter, _contactBuffer);
            contacts = _contactBuffer.AsSpan(0, contactCount);
            return !contacts.IsEmpty;
        }


        /* Project collider along given path. */
        public bool CastCircle(Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
        {
            hit = default;

            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;

            if (_circleCollider.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            Physics2D.queriesStartInColliders = queriesStartInColliders;
            return hit;
        }
        
        /* Cast a line at a specific collider, ignoring everything else. */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
        {
            if (collider == null)
            {
                hit = default;
                return false;
            }

            int layer = collider.gameObject.layer;
            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            LayerMask includeLayers = _contactFilter.layerMask;
            collider.gameObject.layer = Physics2D.IgnoreRaycastLayer;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;
            _contactFilter.SetLayerMask(~collider.gameObject.layer);

            int hitCount = Physics2D.Linecast(origin, origin + distance * direction, _contactFilter, _hitBuffer);

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
            return hit;
        }
        
        
        /* Query existing contacts from last physics pass. If in middle of fixedUpdate after modifying position, will need to syncTransforms. */
        public ColliderDistance2D ComputeMinimumSeparation(Collider2D collider)
        {
            if (collider == null)
            {
                return default;
            }
            ColliderDistance2D minimumSeparation = _circleCollider.Distance(collider);
            return minimumSeparation.isValid ? minimumSeparation : default;
        }
    }
}

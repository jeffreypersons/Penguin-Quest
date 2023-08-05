using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_003
{
    [Flags]
    public enum ContactFlags2D
    {
        None              = 0,
        LeftSide          = 1 << 1,
        BottomLeftCorner  = 1 << 2,
        BottomSide        = 1 << 3,
        BottomRightCorner = 1 << 4,
        RightSide         = 1 << 5,
        TopRightCorner    = 1 << 6,
        TopSide           = 1 << 7,
        TopLeftCorner     = 1 << 8,
        All               = ~0,
    }

    internal sealed class Body
    {
        private Transform        _transform;
        private Rigidbody2D      _rigidbody;
        private BoxCollider2D    _boxCollider;
        private ContactFilter2D  _contactFilter;
        private RaycastHit2D[]   _hitBuffer;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;

        public Vector2 Position => _rigidbody.position;
        public Vector2 Extents  => _boxCollider.bounds.extents;
        public Vector2 Forward  => _transform.right.normalized;
        public Vector2 Up       => _transform.up.normalized;

        private const int DefaultBufferSize = 16;

        public bool IsFlippedHorizontal => _rigidbody.transform.localEulerAngles.y >= 90f;
        public bool IsFlippedVertical   => _rigidbody.transform.localEulerAngles.x >= 90f;

        public Body(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!transform.TryGetComponent(out Rigidbody2D rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(transform)}");
            }
            if (!transform.TryGetComponent(out BoxCollider2D boxCollider))
            {
                throw new MissingComponentException($"Expected attached {nameof(BoxCollider2D)} - not found on {nameof(transform)}");
            }
            if (!ReferenceEquals(boxCollider.attachedRigidbody, rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(boxCollider)}");
            }

            _transform     = rigidbody2D.transform;
            _rigidbody     = rigidbody2D;
            _boxCollider   = boxCollider;
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

        public ContactFlags2D CheckSides()
        {
            _contactFilter.useNormalAngle = true;

            const float epsilon = 0.005f;
            bool isDiagonal = false;
            int degrees = 0;
            ContactFlags2D flags = ContactFlags2D.None;
            for (int i = 0; i < 7; i++)
            {
                if (isDiagonal)
                {
                    _contactFilter.SetNormalAngle(degrees - 45 + epsilon, degrees + 45 - epsilon);
                }
                else
                {
                    _contactFilter.SetNormalAngle(degrees - epsilon, degrees + epsilon);
                }
                
                if (_boxCollider.IsTouching(_contactFilter))
                {
                    flags |= (ContactFlags2D)(1 << (i+1));
                }

                degrees += 45;
                isDiagonal = !isDiagonal;
            }

            var a = Physics2D.Raycast(Position, Vector2.up);
            if (a)
            {
                Debug.Log($"{a.collider.name} {a.distance}");
            }
            _contactFilter.useNormalAngle = false;
            return flags;
        }

        /*
        Project a point along given direction until specific given collider is hit.

        Note that in 3D we have collider.RayCast for this, but in 2D we have no built in way of checking a
        specific collider (collider2D.RayCast confusingly casts _from_ it instead of _at_ it).
        */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int layer = collider.gameObject.layer;
            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            LayerMask includeLayers = _contactFilter.layerMask;

            collider.gameObject.layer = Physics2D.IgnoreRaycastLayer;
            Physics2D.queriesStartInColliders = true;
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
            return hit;
        }

        /* Check for overlapping colliders within our bounding box. */
        public bool CheckForOverlappingColliders(out ReadOnlySpan<Collider2D> colliders)
        {
            int colliderCount = _boxCollider.Overlap(_contactFilter, _overlapBuffer);
            colliders = _overlapBuffer.AsSpan(0, colliderCount);
            return !colliders.IsEmpty;
        }
    }
}

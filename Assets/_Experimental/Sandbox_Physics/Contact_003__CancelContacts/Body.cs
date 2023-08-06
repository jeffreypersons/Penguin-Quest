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

        private const float DefaultEpsilon = 0.005f;
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

            bool isDiagonal = false;
            int degrees = 0;
            ContactFlags2D flags = ContactFlags2D.None;
            for (int i = 0; i < 7; i++)
            {
                if (isDiagonal)
                {
                    _contactFilter.SetNormalAngle(degrees - 45 + DefaultEpsilon, degrees + 45 - DefaultEpsilon);
                }
                else
                {
                    _contactFilter.SetNormalAngle(degrees - DefaultEpsilon, degrees + DefaultEpsilon);
                }
                
                if (_boxCollider.IsTouching(_contactFilter))
                {
                    flags |= (ContactFlags2D)(1 << (i+1));
                }

                degrees += 45;
                isDiagonal = !isDiagonal;
            }
            _contactFilter.useNormalAngle = false;
            return flags;
        }

        /*
        Check if body is contained within an edge.
        
        Considered to be 'inside' if there is an edge collider above center of our AABB, and the same edge collider below.
        Assumes there aren't any edge collider inside another.
        */
        public bool IsInsideAnEdgeCollider(out EdgeCollider2D collider)
        {
            var aboveHit = CastRay(Vector2.up);
            if (!aboveHit || aboveHit.collider is not EdgeCollider2D)
            {
                collider = default;
                return false;
            }

            collider = aboveHit.collider as EdgeCollider2D;
            var belowHit = CastRayAt(collider, Vector2.down, distance: 2f * collider.bounds.extents.y);
            if (!belowHit || belowHit.collider != collider)
            {
                return false;
            }

            return true;
        }
        
        /*
        Project point along given direction and local offset from AABB center, and return first hit (if any).
        */
        public RaycastHit2D CastRay(Vector2 direction, float distance=Mathf.Infinity, Vector2? centerOffset=null)
        {
            int layer = _boxCollider.gameObject.layer;
            _boxCollider.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            Vector2 origin = (Vector2)_boxCollider.bounds.center + centerOffset.GetValueOrDefault(Vector2.zero);
            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);

            _boxCollider.gameObject.layer = layer;
            return hitCount > 1 ? _hitBuffer[0] : default;
        }
        
        /*
        Project point along given direction and local offset from AABB center, and return first hit (if any).
        */
        public RaycastHit2D CastRayAt(Collider2D collider, Vector2 direction, float distance=Mathf.Infinity, Vector2? centerOffset=null)
        {
            int layer = _boxCollider.gameObject.layer;
            _boxCollider.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            Vector2 origin = (Vector2)_boxCollider.bounds.center + centerOffset.GetValueOrDefault(Vector2.zero);
            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);

            _boxCollider.gameObject.layer = layer;
            return hitCount > 1 ? _hitBuffer[0] : default;
        }
    }
}

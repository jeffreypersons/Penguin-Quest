using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_005
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

        
        /* Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width. */
        public ContactFlags2D CheckForOverlappingContacts(float skinWidth)
        {
            Transform transform = _rigidbody.transform;
            Vector2 right = transform.right.normalized;
            Vector2 up    = transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            ContactFlags2D flags = ContactFlags2D.None;
            if (CastAABB(right, skinWidth, out _))
            {
                flags |= ContactFlags2D.RightSide;
            }
            if (CastAABB(up, skinWidth, out _))
            {
                flags |= ContactFlags2D.TopSide;
            }
            if (CastAABB(left, skinWidth, out _))
            {
                flags |= ContactFlags2D.LeftSide;
            }
            if (CastAABB(down, skinWidth, out _))
            {
                flags |= ContactFlags2D.BottomSide;
            }

            #if UNITY_EDITOR
            Bounds bounds = _boxCollider.bounds;
            Vector2 center    = new Vector2(bounds.center.x, bounds.center.y);
            Vector2 skinRatio = new Vector2(1f + (skinWidth / bounds.extents.x), 1f + (skinWidth / bounds.extents.y));
            Vector2 xAxis     = bounds.extents.x * right;
            Vector2 yAxis     = bounds.extents.y * up;

            float duration = Time.fixedDeltaTime;
            Debug.DrawLine(center + xAxis, center + skinRatio * xAxis, Color.magenta, duration);
            Debug.DrawLine(center - xAxis, center - skinRatio * xAxis, Color.magenta, duration);
            Debug.DrawLine(center + yAxis, center + skinRatio * yAxis, Color.magenta, duration);
            Debug.DrawLine(center - yAxis, center - skinRatio * yAxis, Color.magenta, duration);
            #endif
            return flags;
        }

        /*
        Check if body center is fully surrounded by the same edge collider.
        
        Considered to be 'inside' if there is an edge collider above center of our AABB, and the same edge collider below.
        Assumes there aren't any edge collider inside another.
        */
        public bool IsCenterBoundedByAnEdgeCollider(out EdgeCollider2D collider)
        {
            collider = default;

            if (!CastRay(Vector2.up, Mathf.Infinity, out var aboveHit) ||
                !aboveHit.collider.transform.TryGetComponent<EdgeCollider2D>(out var edge))
            {
                return false;
            }

            float maxHorizontal = 2f * edge.bounds.extents.x;
            float maxVertical   = 2f * edge.bounds.extents.y;
            if (!CastRayAt(edge, Vector2.down,  maxVertical,   out var _) ||
                !CastRayAt(edge, Vector2.left,  maxHorizontal, out var _) ||
                !CastRayAt(edge, Vector2.right, maxHorizontal, out var _))
            {
                return false;
            }

            collider = edge;
            return true;
        }
        
        /*
        Project center point along given direction, outputting first hit (if any).
        */
        public bool CastRay(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int layer = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            Vector2 origin = _rigidbody.position;

            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            else
            {
                hit = default;
            }

            _transform.gameObject.layer = layer;

            Debug.DrawLine(_rigidbody.position, _rigidbody.position + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(_rigidbody.position, hit.point, Color.green, 1f);
            }
            return hit;
        }
        
        /*
        Project center point along given direction, outputting first hit to given collider (if any).
        */
        public bool CastRayAt(Collider2D collider, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int layer = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            hit = default;
            int hitCount = Physics2D.Raycast(_rigidbody.position, direction, _contactFilter, _hitBuffer, distance);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider == collider)
                {
                    hit = _hitBuffer[i];
                    break;
                }
            }

            _transform.gameObject.layer = layer;

            Debug.DrawLine(_rigidbody.position, _rigidbody.position + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(_rigidbody.position, hit.point, Color.green, 1f);
            }
            return hit;
        }

        /* Project a rectangle along delta, ignoring ALL attached colliders, and stopping at first hit (if any). */
        public bool CastAABB(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            // note that there is no need to disable colliders as that is accounted for by collider instance
            if (_rigidbody.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }

            #if UNITY_EDITOR
            // note that this won't be a perfect representation as it doesn't account for the rounded edges,
            // but close enough for our purposes to visualize the cast
            Vector2 center  = _boxCollider.bounds.center;
            Vector2 extents = _boxCollider.bounds.extents + new Vector3(_boxCollider.edgeRadius, _boxCollider.edgeRadius, 0f);
            DebugExtensions.DrawBoxCast(center, extents, 0f, direction, distance, _hitBuffer.AsSpan(1), Time.fixedDeltaTime);
            #endif
            return hit;
        }
    }
}

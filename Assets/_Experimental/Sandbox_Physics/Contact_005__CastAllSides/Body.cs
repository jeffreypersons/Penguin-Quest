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

        private LayerMask _previousLayerMask;

        public Vector2 Position => _rigidbody.position;
        public Vector2 Extents  => _boxCollider.bounds.extents;
        public Vector2 Forward  => _transform.right.normalized;
        public Vector2 Up       => _transform.up.normalized;

        private const float DefaultEpsilon = 0.005f;
        private const int DefaultBufferSize = 16;
        private readonly Vector2 NormalizedDiagonal = Vector2.one.normalized;

        public bool IsFlippedHorizontal => _rigidbody.transform.localEulerAngles.y >= 90f;
        public bool IsFlippedVertical   => _rigidbody.transform.localEulerAngles.x >= 90f;


        private void DisableCollisionsWithAABB()
        {
            _previousLayerMask = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;
        }

        private void ReEnableCollisionsWithAABB()
        {
            _transform.gameObject.layer = _previousLayerMask;
        }


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
            if (CheckDirection(right, skinWidth, out _))
            {
                flags |= ContactFlags2D.RightSide;
            }
            if (CheckDirection(up, skinWidth, out _))
            {
                flags |= ContactFlags2D.TopSide;
            }
            if (CheckDirection(left, skinWidth, out _))
            {
                flags |= ContactFlags2D.LeftSide;
            }
            if (CheckDirection(down, skinWidth, out _))
            {
                flags |= ContactFlags2D.BottomSide;
            }

            if (CheckDirection(new Vector2(1, -1) * NormalizedDiagonal, skinWidth, out _))
            {
                flags |= ContactFlags2D.BottomRightCorner;
            }
            if (CheckDirection(new Vector2(1, 1) * NormalizedDiagonal, skinWidth, out _))
            {
                flags |= ContactFlags2D.TopRightCorner;
            }
            if (CheckDirection(new Vector2(-1, 1) * NormalizedDiagonal, skinWidth, out _))
            {
                flags |= ContactFlags2D.TopLeftCorner;
            }
            if (CheckDirection(new Vector2(-1, -1) * NormalizedDiagonal, skinWidth, out _))
            {
                flags |= ContactFlags2D.BottomLeftCorner;
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
        
        public bool CastRay(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            if (Physics2D.Raycast(_rigidbody.position, direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }
            ReEnableCollisionsWithAABB();
            DebugExtensions.DrawRayCast(_rigidbody.position, direction, distance, hit, Time.fixedDeltaTime);
            return hit;
        }
        
        public bool CastRayAt(Collider2D collider, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
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
            ReEnableCollisionsWithAABB();
            DebugExtensions.DrawRayCast(_rigidbody.position, direction, distance, hit, Time.fixedDeltaTime);
            return hit;
        }

        public bool CheckDirection(Vector2 direction, float distance, out RaycastHit2D hit)
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
            DebugExtensions.DrawBoxCast(_boxCollider.bounds.center, _boxCollider.bounds.extents, 0f, direction, distance, _hitBuffer.AsSpan(1), Time.fixedDeltaTime);
            return hit;
        }
    }
}

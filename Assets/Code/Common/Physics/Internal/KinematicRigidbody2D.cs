using System;
using UnityEngine;


namespace PQ.Common.Physics.Internal
{
    /*
    Internal container for unifying physics calls.

    Notes
    * Assumes always upright bounding box, with kinematic rigidbody
    * Corresponding game object is fixed in rotation to enforce alignment with global up
    * Caching is done only for cast results, position caching is intentionally left to any calling code
    */
    internal sealed class KinematicRigidbody2D
    {
        private Transform        _transform;
        private Rigidbody2D      _rigidbody;
        private BoxCollider2D    _boxCollider;
        private ContactFilter2D  _contactFilter;
        private RaycastHit2D[]   _hitBuffer;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;

        private float _bounciness   = 0.00f;
        private float _friction     = 0.00f;
        private float _gravityScale = 1.00f;
        private const int DefaultBufferSize = 16;


        public override string ToString() =>
            $"{GetType()}{{" +
                $"Transform:{_transform.name}" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB:bounds(center:{Center},extents:{Extents})," +
                $"Gravity:{GravityScale}," +
                $"OverlapTolerance:{SkinWidth}," +
                $"Friction:{Friction}," +
                $"LayerMask:{LayerMask}," +
            $"}}";

        // todo: cache these
        public LayerMask LayerMask => _contactFilter.layerMask;

        public float   Friction     => _friction;
        public float   Bounciness   => _bounciness;
        public float   GravityScale => _gravityScale;

        public Vector2 Position     => _rigidbody.position;
        public Vector2 Center       => _boxCollider.bounds.center;
        public Vector2 Forward      => _rigidbody.transform.right.normalized;
        public Vector2 Up           => _rigidbody.transform.up.normalized;
        public Vector2 Extents      => _boxCollider.bounds.extents + new Vector3(_boxCollider.edgeRadius, _boxCollider.edgeRadius, 0f);
        public float   Depth        => _rigidbody.transform.position.z;
        public float   SkinWidth    => _boxCollider.edgeRadius;
        public Vector2 BoundsOffset => _boxCollider.offset;

        public bool IsFlippedHorizontal => _rigidbody.transform.localEulerAngles.y >= 90f;
        public bool IsFlippedVertical   => _rigidbody.transform.localEulerAngles.x >= 90f;
        
        #if UNITY_EDITOR
        public bool DrawCastsInEditor { get; set; } = true;
        #endif

        public KinematicRigidbody2D(Transform transform)
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

            _contactFilter.useTriggers    = true;
            _contactFilter.useLayerMask   = true;
            _contactFilter.useNormalAngle = true;

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }


        public void ResizeHitBuffer(int length)
        {
            Array.Resize(ref _hitBuffer,     length);
            Array.Resize(ref _overlapBuffer, length);
            Array.Resize(ref _contactBuffer, length);
        }

        public void SetLocalBounds(Vector2 offset, Vector2 size, float outerEdgeRadius)
        {
            if (_boxCollider.offset != offset ||
                _boxCollider.size   != size   ||
                !Mathf.Approximately(_boxCollider.edgeRadius, outerEdgeRadius))
            {
                _boxCollider.offset     = offset;
                _boxCollider.size       = size;
                _boxCollider.edgeRadius = outerEdgeRadius;
            }
        }

        /* Given amount between 0 and 1, set rotation about y axis, and rotation about x axis. Note we never allow z rotation. */
        public void SetFlippedAmount(float horizontalRatio, float verticalRatio)
        {
            _rigidbody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.transform.localEulerAngles = new Vector3(
                x: verticalRatio   * 180f,
                y: horizontalRatio * 180f,
                z: 0f);
            _rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }


        public void SetPhysicalProperties(float friction, float bounciness, float gravityScale)
        {
            _friction     = friction;
            _bounciness   = bounciness;
            _gravityScale = gravityScale;
        }

        public void SetLayerMask(LayerMask layerMask) => _contactFilter.SetLayerMask(layerMask);
        public bool IsAttachedTo(Transform transform) => ReferenceEquals(_transform, transform);

        public void TeleportTo(Vector2 position) => _transform.position = position;
        public void MoveTo(Vector2 position)     => _rigidbody.position = position;
        public void MoveBy(Vector2 delta)        => _rigidbody.position += delta;

        /*
        Move body to given frame's start position and perform MovePosition to maintain any interpolation.
        
        This allows changes to rigidbody.position be applied without ignoring interpolation settings.
        
        Context:
        - Interpolation smooths movement based on past frame positions (eg useful for player input driven gameobjects)
        - For kinematic rigidbodies, this only works if position is changed via rigidbody.MovePosition() in FixedUpdate()
        - To interpolate movement despite modifying rigidbody.position (eg performing physics by hand),
          replace the original position _then_ apply MovePosition()
        */
        public void MovePosition(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
        {
            _rigidbody.position = startPositionThisFrame;
            _rigidbody.MovePosition(targetPositionThisFrame);
        }


        /*
        Project AABB along delta, and return ALL hits (if any).
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        public bool CastAABB_All(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
        {
            if (delta == Vector2.zero)
            {
                hits = _hitBuffer.AsSpan(0, 0);
                return false;
            }

            Bounds bounds = _boxCollider.bounds;

            Vector2 center    = bounds.center;
            Vector2 size      = bounds.size - new Vector3(2f * _boxCollider.edgeRadius, 2f * _boxCollider.edgeRadius, 0f);
            float   distance  = delta.magnitude;
            Vector2 direction = delta / distance;

            int hitCount = Physics2D.BoxCast(center, size, 0, direction, _contactFilter, _hitBuffer, distance);
            hits = _hitBuffer.AsSpan(0, hitCount);
            
            #if UNITY_EDITOR
            if (DrawCastsInEditor)
            {
                float duration = Time.fixedDeltaTime;
                foreach (RaycastHit2D hit in hits)
                {
                    Vector2 edgePoint = hit.point - (hit.distance * direction);
                    Vector2 hitPoint  = hit.point;
                    Vector2 endPoint  = hit.point + (distance * direction);

                    Debug.DrawLine(edgePoint, hitPoint, Color.green, duration);
                    Debug.DrawLine(hitPoint,  endPoint, Color.red,   duration);
                }
            }
            #endif
            return !hits.IsEmpty;
        }


        /*
        Project AABB along delta, and return CLOSEST hit (if any).
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        public bool CastAABB_Closest(Vector2 delta, out RaycastHit2D hit)
        {
            if (!CastAABB_All(delta, out ReadOnlySpan<RaycastHit2D> hits))
            {
                hit = default;
                return false;
            }

            int closestHitIndex = 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].distance < hits[closestHitIndex].distance)
                {
                    closestHitIndex = i;
                }
            }
            hit = hits[closestHitIndex];
            return true;
        }

        /*
        Check for overlapping colliders within our bounding box.
        */
        public bool CheckForOverlappingColliders(out ReadOnlySpan<Collider2D> colliders)
        {
            int colliderCount = _boxCollider.OverlapCollider(_contactFilter, _overlapBuffer);
            colliders = _overlapBuffer.AsSpan(0, colliderCount);
            return !colliders.IsEmpty;
        }

        /*
        Query existing contacts from last physics pass.
        */
        public bool CheckForContacts(out ReadOnlySpan<ContactPoint2D> contacts)
        {
            int contactCount = _boxCollider.GetContacts(_contactFilter, _contactBuffer);
            contacts = _contactBuffer.AsSpan(0, contactCount);
            return !contacts.IsEmpty;
        }

        /*
        Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width.

        If no layermask provided, uses the one assigned in editor.
        */
        public CollisionFlags2D CheckSides()
        {
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
            Debug.Log(flags);
            return flags;
        }

        /*
        Compute distance from center to edge of our bounding box in given direction.
        */
        public float ComputeDistanceToEdge(Vector2 direction)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.IntersectRay(new Ray(bounds.center, direction), out float distanceFromCenterToEdge);
            return distanceFromCenterToEdge;
        }

        /*
        Compute vector representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for
        complex polygons.
        */
        public ColliderDistance2D ComputeMinimumSeparation(Collider2D collider)
        {
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (!minimumSeparation.isValid)
            {
                throw new InvalidOperationException("Error state - invalid minimum separation between body and given collider");
            }
            return minimumSeparation;
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
    }
}

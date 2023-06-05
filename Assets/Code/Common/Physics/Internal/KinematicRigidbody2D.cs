using System;
using UnityEngine;
using PQ.Common.Extensions;


// todo: cache list of all colliders attached to rigidbody, so these can be toggled on/off for certain casts/etc
namespace PQ.Common.Physics.Internal
{
    /*
    Internal container for unifying physics calls.

    Notes
    * Assumes always upright bounding box, with kinematic rigidbody
    * Corresponding game object is fixed in rotation to enforce alignment with global up
    * Caching is done only for cast results, position caching is intentionally left to any calling code
    * Any result (from physics queries) are intended to be used right away, as any subsequent casts may change the result(s)
    * Assumes any given direction is normalized
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
        private const float DefaultRayMaxDistance = 100f;


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
        public bool DrawShapeCastsInEditor { get; set; } = true;
        public bool DrawRayCastsInEditor   { get; set; } = true;
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

            _contactFilter.useTriggers    = false;
            _contactFilter.useLayerMask   = true;
            _contactFilter.useNormalAngle = false;

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;
        }


        public void ResizeHitBuffer(int length)
        {
            Array.Resize(ref _hitBuffer,     length);
            Array.Resize(ref _overlapBuffer, length);
            Array.Resize(ref _contactBuffer, length);
        }
        
        public void SetPhysicalProperties(float friction, float bounciness, float gravityScale)
        {
            _friction     = friction;
            _bounciness   = bounciness;
            _gravityScale = gravityScale;
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
            // todo: when we do lerped rotations we will need to likely utilize _rigidbody.transform.right.normalized, etc
            _rigidbody.transform.localEulerAngles = new Vector3(
                x: verticalRatio   * 180f,
                y: horizontalRatio * 180f,
                z: 0f);
        }

        public void SetConstraints(RigidbodyConstraints2D constraints)
        {
            _rigidbody.constraints = constraints;
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
        Project line along given delta and local offset from AABB center, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastRay(Vector2 centerOffset, Vector2 direction, float distance, out ReadOnlySpan<RaycastHit2D> hits)
        {
            Vector2 origin = (Vector2)_boxCollider.bounds.center + centerOffset;

            _boxCollider.enabled = false;
            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);
            hits = _hitBuffer.AsSpan(0, hitCount);
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
        Cast AABB along given delta from AABB center, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastAABB(Vector2 direction, float distance, out ReadOnlySpan<RaycastHit2D> hits, bool includeAlreadyOverlappingColliders)
        {
            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;

            Bounds bounds = _boxCollider.bounds;

            Vector2 center = bounds.center;
            Vector2 size   = bounds.size;

            int hitCount = _boxCollider.Cast(direction, _contactFilter, _hitBuffer, distance, ignoreSiblingColliders: true);
            hits = _hitBuffer.AsSpan(0, hitCount);

            #if UNITY_EDITOR
            if (DrawShapeCastsInEditor)
            {
                float duration = Time.fixedDeltaTime;
                DebugExtensions.DrawBoxCast(center, 0.50f * size, 0f, direction, distance, hits, 0.5f);
            }
            #endif
            
            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;
            return !hits.IsEmpty;
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

        /*
        Compute signed distance representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for complex polygons.
        */
        public float ComputeSeparationAlongDelta(Collider2D collider, Vector2 direction)
        {
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (collider == null || !minimumSeparation.isValid)
            {
                throw new InvalidOperationException("Error state - invalid minimum separation between body and given collider");
            }

            Vector2 minOffset = minimumSeparation.distance * minimumSeparation.normal;
            if (minOffset == Vector2.zero)
            {
                return 0f;
            }

            Vector2 pointA = minimumSeparation.pointA;
            Vector2 pointB = minimumSeparation.pointB;
            Vector2 closestAABBPoint = Vector2.LerpUnclamped(pointA, pointB, 0.50f) + minOffset;
            Vector2 directionToSurface = minimumSeparation.isOverlapped ? -direction : direction;

            RaycastHit2D hit = default;
            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = true;
            int hitCount = Physics2D.Raycast(closestAABBPoint, directionToSurface, _contactFilter, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (ReferenceEquals(_hitBuffer[i].collider, collider))
                {
                    hit = _hitBuffer[i];
                    break;
                }
            }
            if (!hit)
            {
                Debug.LogWarning("This shouldn't happen!");
            }
            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;

            // todo: should we use collider's closest point method as well to get a more precise edge point?
            float separation = hit? hit.distance : 0f;
            
            #if UNITY_EDITOR
            if (DrawRayCastsInEditor)
            {
                float duration = 2f;
                DebugExtensions.DrawRayCast(closestAABBPoint, direction, minOffset.magnitude, hit, duration);
                DebugExtensions.DrawLine(minimumSeparation.pointA, minimumSeparation.pointB, Color.magenta, duration);
            }
            #endif
            return separation;
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

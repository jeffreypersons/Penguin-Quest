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
        private Transform       _transform;
        private Rigidbody2D     _rigidbody;
        private BoxCollider2D   _boxCollider;
        private ContactFilter2D _contactFilter;
        private RaycastHit2D[]  _hitBuffer;

        private float _bounciness   = 0.00f;
        private float _friction     = 0.00f;
        private float _gravityScale = 1.00f;
        private const int DefaultHitBufferSize = 16;


        public override string ToString() =>
            $"{GetType()}{{" +
                $"Transform:{_transform.name}" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB:bounds(center:{Center},extents:{Extents})," +
                $"Gravity:{GravityScale}," +
                $"SkinWidth:{SkinWidth}," +
                $"Friction:{Friction}," +
                $"LayerMask:{LayerMask}," +
            $"}}";


        // todo: cache these
        public Vector2 Position => _rigidbody.position;
        public Vector2 Center   => _boxCollider.bounds.center;
        public Vector2 Forward  => _rigidbody.transform.right.normalized;
        public Vector2 Up       => _rigidbody.transform.up.normalized;
        public Vector2 Extents  => _boxCollider.bounds.extents + new Vector3(_boxCollider.edgeRadius, _boxCollider.edgeRadius, 0f);
        public float   Depth    => _rigidbody.transform.position.z;
                
        public float Friction     => _friction;
        public float Bounciness   => _bounciness;
        public float GravityScale => _gravityScale;

        public LayerMask LayerMask => _contactFilter.layerMask;
        public float SkinWidth => _boxCollider.edgeRadius;
        public Vector2 LocalBoundsOffset => _boxCollider.offset;

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
            _hitBuffer     = new RaycastHit2D[DefaultHitBufferSize];

            _rigidbody.isKinematic = true;
            _rigidbody.simulated   = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
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
        public void ResizeHitBuffer(int length)       => Array.Resize(ref _hitBuffer, length);
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
        public bool CastAABB(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
        {
            if (delta == Vector2.zero)
            {
                hits = _hitBuffer.AsSpan(0, 0);
                return false;
            }

            Bounds bounds = _boxCollider.bounds;

            Vector2 center    = bounds.center;
            Vector2 size      = bounds.size;
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
        Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width.

        If no layermask provided, uses the one assigned in editor.
        */
        public CollisionFlags2D CheckForOverlappingContacts(float extent)
        {
            Transform transform = _rigidbody.transform;
            Vector2 right = transform.right.normalized;
            Vector2 up    = transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            CollisionFlags2D flags = CollisionFlags2D.None;
            if (CastAABB(extent * right, out _))
            {
                flags |= CollisionFlags2D.Front;
            }
            if (CastAABB(extent * up, out _))
            {
                flags |= CollisionFlags2D.Above;
            }
            if (CastAABB(extent * left, out _))
            {
                flags |= CollisionFlags2D.Behind;
            }
            if (CastAABB(extent * down, out _))
            {
                flags |= CollisionFlags2D.Below;
            }
            
            #if UNITY_EDITOR
            // draw the 'scan-lines' whether we get a cast hit or not
            if (DrawCastsInEditor)
            {
                Bounds bounds = _boxCollider.bounds;
                Vector2 center    = new(bounds.center.x, bounds.center.y);
                Vector2 skinRatio = new(1f + (extent / bounds.extents.x), 1f + (extent / bounds.extents.y));
                Vector2 xAxis     = bounds.extents.x * right;
                Vector2 yAxis     = bounds.extents.y * up;

                float duration = Time.fixedDeltaTime;
                Debug.DrawLine(center + xAxis, center + skinRatio * xAxis, Color.magenta, duration);
                Debug.DrawLine(center - xAxis, center - skinRatio * xAxis, Color.magenta, duration);
                Debug.DrawLine(center + yAxis, center + skinRatio * yAxis, Color.magenta, duration);
                Debug.DrawLine(center - yAxis, center - skinRatio * yAxis, Color.magenta, duration);
            }
            #endif
            return flags;
        }

        /*
        Compute vector representing overlap amount between body and given collider, if any.

        Uses separating axis theorem to determine overlap - may require more invocations for
        complex polygons.
        */
        public ColliderDistance2D ComputeMinimumSeparation(Collider2D collider)
        {
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (collider == !minimumSeparation.isValid)
            {
                throw new InvalidOperationException("Error state - invalid minimum separation between body and given collider");
            }
            return minimumSeparation;
        }
    }
}

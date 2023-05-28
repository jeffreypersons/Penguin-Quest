using System;
using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.Common.Physics.Internal
{
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
            _rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
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

        public void MovePosition(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
        {
            _rigidbody.position = startPositionThisFrame;
            _rigidbody.MovePosition(targetPositionThisFrame);
        }

        public bool CastAABB_All(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
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
                DrawBoxCast(center, size, direction, distance, hits);
                foreach (RaycastHit2D hit in hits)
                {
                    DrawCastHit(hit);
                }
            }
            #endif
            return !hits.IsEmpty;
        }

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


        public CollisionFlags2D CheckForOverlappingContacts(float extent)
        {
            Transform transform = _rigidbody.transform;
            Vector2 right = (2f * extent + _boxCollider.bounds.extents.x) * transform.right.normalized;
            Vector2 up    = (2f * extent + _boxCollider.bounds.extents.y) * transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            CollisionFlags2D flags = CollisionFlags2D.None;
            if (CastAABB_All(right, out _))
            {
                flags |= CollisionFlags2D.Front;
            }
            if (CastAABB_All(up, out _))
            {
                flags |= CollisionFlags2D.Above;
            }
            if (CastAABB_All(left, out _))
            {
                flags |= CollisionFlags2D.Behind;
            }
            if (CastAABB_All(down, out _))
            {
                flags |= CollisionFlags2D.Below;
            }
            
            #if UNITY_EDITOR
            // draw the 'scan-lines' whether we get a cast hit or not
            if (DrawCastsInEditor)
            {
                Bounds bounds = _boxCollider.bounds;
                Vector2 center    = new(bounds.center.x, bounds.center.y);
                Vector2 skinRatio = new(1f + (2f * extent / bounds.extents.x), 1f + (2f * extent / bounds.extents.y));
                Vector2 xAxis     = bounds.extents.x * right;
                Vector2 yAxis     = bounds.extents.y * up;

                float duration = Time.fixedDeltaTime;
                Debug.DrawLine(center + xAxis, center + skinRatio * xAxis, Color.magenta, duration);
                Debug.DrawLine(center - xAxis, center - skinRatio * xAxis, Color.magenta, duration);
                Debug.DrawLine(center + yAxis, center + skinRatio * yAxis, Color.magenta, duration);
                Debug.DrawLine(center - yAxis, center - skinRatio * yAxis, Color.magenta, duration);
            }
            #endif
            Debug.Log($"flags={flags}");
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


        #if UNITY_EDITOR
        private static void DrawCastHit(RaycastHit2D hit)
        {
            float duration = Time.fixedDeltaTime;
            Vector2 direction = (hit.point - hit.centroid).normalized;
            float hitDistance  = hit.distance;
            float castDistance = hit.distance + (hit.distance / (1f - hit.fraction));

            Vector2 edgePoint = hit.point - (hitDistance * direction);
            Vector2 hitPoint  = hit.point;
            Vector2 endPoint  = hit.point + (castDistance * direction);

            Debug.DrawLine(edgePoint, hitPoint, Color.grey, duration);
            Debug.DrawLine(hitPoint,  endPoint, Color.red,  duration);
        }

        private static void DrawBoxCast(Vector2 origin, Vector2 size, Vector2 direction, float distance, ReadOnlySpan<RaycastHit2D> hits)
        {
            float duration = Time.fixedDeltaTime;
            DebugExtensions.DrawRect(origin, 0.50f * size, Color.gray, duration);
            DebugExtensions.DrawRect(origin + distance * direction, 0.50f * size, Color.gray, duration);

            foreach (RaycastHit2D hit in hits)
            {
                Vector2 edgePoint = hit.point - (hit.distance * direction);
                Vector2 hitPoint  = hit.point;
                Vector2 endPoint  = hit.point + (distance * direction);

                DebugExtensions.DrawLine(edgePoint, hitPoint, Color.green, duration);
                DebugExtensions.DrawLine(hitPoint,  endPoint, Color.red, duration);
            }
        }
        #endif
    }
}

using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_002
{
    public class Body : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   _rigidbody;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _layerMask = default;
        [SerializeField] [Range(0, 1)]   private float _skinWidth = 0.075f;
        [SerializeField] [Range(1, 100)] private int _preallocatedBufferSize = 16;

        #if UNITY_EDITOR
        [SerializeField] private bool _drawCastsInEditor = true;        
        [SerializeField] private bool _drawMovesInEditor = true;
        #endif

        private ContactFilter2D _castFilter;
        private RaycastHit2D[]  _hitBuffer;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position  => _rigidbody.position;
        public float   Depth     => _rigidbody.transform.position.z;
        public Bounds  Bounds    => _boxCollider.bounds;
        public Vector2 Forward   => _rigidbody.transform.right.normalized;
        public Vector2 Up        => _rigidbody.transform.up.normalized;
        public float   SkinWidth => _skinWidth;


        void Awake()
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var _))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<BoxCollider2D>(out var _))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {transform}");
            }

            _castFilter = new ContactFilter2D();
            _hitBuffer  = new RaycastHit2D[_preallocatedBufferSize];
            _castFilter.useLayerMask = true;
            _castFilter.SetLayerMask(_layerMask);

            float buffer = Mathf.Clamp01(_skinWidth);
            _boxCollider.edgeRadius = buffer;
            _boxCollider.size       = new Vector2(1f - buffer, 1f - buffer);

            _rigidbody.isKinematic = true;
            _rigidbody.simulated   = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }


        /* Immediately set facing of horizontal/vertical axes. */
        public void Flip(bool horizontal, bool vertical)
        {
            _rigidbody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.transform.localEulerAngles = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);
            _rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        /* Set world transform to given point, ignoring physics. */
        public void TeleportTo(Vector2 position)
        {
            transform.position = position;
        }

        /* Immediately move body to given point. */
        public void MoveTo(Vector2 position)
        {
            #if UNITY_EDITOR
            if (_drawMovesInEditor)
            {
                Debug.DrawLine(_rigidbody.position, position, Color.grey, Time.fixedDeltaTime);
            }
            #endif
            _rigidbody.position = position;
        }

        /* Immediately move body by given amount. */
        public void MoveBy(Vector2 delta)
        {
            #if UNITY_EDITOR
            if (_drawMovesInEditor)
            {
                Debug.DrawLine(_rigidbody.position, _rigidbody.position + delta, Color.grey, Time.fixedDeltaTime);
            }
            #endif
            _rigidbody.position += delta;
        }

        /*
        Move body to given frame's start position and perform MovePosition to maintain any interpolation.
        
        Context:
        - Interpolation smooths movement based on past frame positions (eg useful for player input driven gameobjects)
        - For kinematic rigidbodies, this only works if position is changed via rigidbody.MovePosition() in FixedUpdate()
        - To interpolate movement despite modifying rigidbody.position (eg performing physics by hand),
          replace the original position _then_ apply MovePosition()

        Reference:
        - https://illogika-studio.gitbooks.io/unity-best-practices/content/physics-rigidbody-interpolation-and-fixedtimestep.html

        Warning:
        - Only use if you know exactly what you are doing with physics
        - Interpolation can still be broken if position directly modified again in the same physics frame
        */
        public void InterpolatedMoveTo(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
        {
            // todo: look into encapsulating this inside a FixedUpdate call and a KinematicBody interpolation mode instead
            //       would require storing any changes for current frame and 'undoing' and repllaying via MovePosition() like below
            #if UNITY_EDITOR
            if (_drawMovesInEditor)
            {
                Debug.DrawLine(startPositionThisFrame, targetPositionThisFrame, Color.grey, Time.fixedDeltaTime);
            }
            #endif
            _rigidbody.position = startPositionThisFrame;
            _rigidbody.MovePosition(targetPositionThisFrame);
        }

        /*
        Project AABB along delta, and return ALL hits (if any).
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        public bool CastAABB(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
        {
            _castFilter.SetLayerMask(_layerMask);

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

            int hitCount = Physics2D.BoxCast(center, size, 0, direction, _castFilter, _hitBuffer, distance);
            hits = _hitBuffer.AsSpan(0, hitCount);

            #if UNITY_EDITOR
            if (_drawCastsInEditor)
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
        Project a point along given direction until specific given collider is hit.

        Note that in 3D we have collider.RayCast for this, but in 2D we have no built in way of checking a
        specific collider (collider2D.RayCast confusingly casts _from_ it instead of _at_ it).
        */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int layer = collider.gameObject.layer;
            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            LayerMask includeLayers = _castFilter.layerMask;

            collider.gameObject.layer = Physics2D.IgnoreRaycastLayer;
            Physics2D.queriesStartInColliders = true;
            _castFilter.SetLayerMask(~collider.gameObject.layer);

            int hitCount = Physics2D.Raycast(origin, direction, _castFilter, _hitBuffer, distance);

            collider.gameObject.layer = layer;
            _castFilter.SetLayerMask(includeLayers);
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
        
        /*
        Determine whether our body's AABB is fully inside given collider.

        We don't worry about strange cases like a donut collider - center and corners encapsulated is 'good enough'.
        Note this doesn't work with edge colliders since there is no 'internal area' to test.
        */
        public bool IsFullyEncapsulatedBy(Collider2D collider)
        {
            float x = _boxCollider.bounds.center.x;
            float y = _boxCollider.bounds.center.y;
            float halfWidth  = _boxCollider.bounds.extents.x + _boxCollider.edgeRadius;
            float halfHeight = _boxCollider.bounds.extents.y + _boxCollider.edgeRadius;
            return collider.OverlapPoint(new Vector2(x, y)) &&
                   collider.OverlapPoint(new Vector2(x + halfWidth, y + halfHeight)) &&
                   collider.OverlapPoint(new Vector2(x + halfWidth, y - halfHeight)) &&
                   collider.OverlapPoint(new Vector2(x - halfWidth, y - halfHeight)) &&
                   collider.OverlapPoint(new Vector2(x - halfWidth, y + halfHeight));
        }


        /* Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width. */
        public CollisionFlags2D CheckForOverlappingContacts(float skinWidth)
        {
            Transform transform = _rigidbody.transform;
            Vector2 right = transform.right.normalized;
            Vector2 up    = transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            CollisionFlags2D flags = CollisionFlags2D.None;
            if (CastAABB(skinWidth * right, out _))
            {
                flags |= CollisionFlags2D.Front;
            }
            if (CastAABB(skinWidth * up, out _))
            {
                flags |= CollisionFlags2D.Above;
            }
            if (CastAABB(skinWidth * left, out _))
            {
                flags |= CollisionFlags2D.Behind;
            }
            if (CastAABB(skinWidth * down, out _))
            {
                flags |= CollisionFlags2D.Below;
            }
            
            #if UNITY_EDITOR
            if (_drawCastsInEditor)
            {
                Bounds bounds = _boxCollider.bounds;
                Vector2 center    = new(bounds.center.x, bounds.center.y);
                Vector2 skinRatio = new(1f + (skinWidth / bounds.extents.x), 1f + (skinWidth / bounds.extents.y));
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
            if (collider == null)
            {
                return default;
            }
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            return minimumSeparation.isValid ? minimumSeparation : default;
        }


        void OnValidate()
        {
            _boxCollider.edgeRadius = _skinWidth;
            _boxCollider.size       = new Vector2(1f - _skinWidth, 1f - _skinWidth);

            if (!Application.IsPlaying(this))
            {
                return;
            }

            if (_hitBuffer == null || _preallocatedBufferSize != _hitBuffer.Length)
            {
                _hitBuffer = new RaycastHit2D[_preallocatedBufferSize];
            }
        }

        void OnDrawGizmos()
        {
            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // surrounded by an outer bounding box offset by our skin with, with a pair of arrows from the that
            // should be identical to the transform's axes in the editor window
            Bounds box = Bounds;
            Vector2 center    = new(box.center.x, box.center.y);
            Vector2 skinRatio = new(1f + (_skinWidth / box.extents.x), 1f + (_skinWidth / box.extents.y));
            Vector2 xAxis     = box.extents.x * Forward;
            Vector2 yAxis     = box.extents.y * Up;

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.black);
            GizmoExtensions.DrawRect(center, skinRatio.x * xAxis, skinRatio.y * yAxis, Color.black);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }
    }
}

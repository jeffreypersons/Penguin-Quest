using System;
using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.Common.Physics
{
    /*
    Represents a physical body aligned with an AABB and driven by kinematic physics.

    Notes
    * Assumes always upright bounding box, with kinematic rigidbody
    * Corresponding game object is fixed in rotation to enforce alignment with global up
    * Caching is done only for cast results, position caching is intentionally left to any calling code
    */
    [AddComponentMenu("KinematicBody2D")]
    public sealed class KinematicBody2D : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   _rigidBody;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _layerMask = default;
        [SerializeField] [Range(0, 1)]   private float _skinWidth = 0.01f;
        [SerializeField] [Range(1, 100)] private int _preallocatedHitBufferSize = 16;

        #if UNITY_EDITOR
        [SerializeField] private bool _drawCastsInEditor = true;        
        [SerializeField] private bool _drawMovesInEditor = true;
        #endif

        private bool _initialized;
        private bool _flippedHorizontal;
        private bool _flippedVertical;

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

        public bool    FlippedHorizontal => _flippedHorizontal;
        public bool    FlippedVertical   => _flippedVertical;
        public Vector2 Position          => _rigidBody.position;
        public float   Depth             => _rigidBody.transform.position.z;
        public Bounds  Bounds            => _boxCollider.bounds;
        public float   SkinWidth         => _skinWidth;
        public Vector2 Forward           => _rigidBody.transform.right.normalized;
        public Vector2 Up                => _rigidBody.transform.up.normalized;

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
            _hitBuffer  = new RaycastHit2D[_preallocatedHitBufferSize];

            _rigidBody.isKinematic = true;
            _rigidBody.simulated   = true;
            _rigidBody.useFullKinematicContacts = true;
            _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

            _initialized = true;
        }


        /* Set layermask used for detecting collisions. */
        public void SetLayerMask(LayerMask layerMask)
        {
            _layerMask = layerMask;
            _castFilter.SetLayerMask(layerMask);
        }

        /* Resize AABB to span between given local coordinates, with skin width as our collision contact offset. */
        public void SetBounds(Vector2 from, Vector2 to, float skinWidth)
        {
            Vector2 center = Vector2.LerpUnclamped(from, to, 0.50f);
            Vector2 size   = new Vector2(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
            Vector2 buffer = new Vector2(2f * skinWidth, 2f * skinWidth);
            if (size.x <= 0 || size.y <= 0)
            {
                throw new ArgumentOutOfRangeException($"Invalid bounds - expected size >= 0, received from={from} and to={to}");
            }
            if (skinWidth < 0f || buffer.x >= size.x || buffer.y >= size.y)
            {
                throw new ArgumentOutOfRangeException($"Invalid skin-width - expected >= 0 and < size={size}, received skinWidth={skinWidth}");
            }

            _boxCollider.size = size - buffer;
            _boxCollider.offset = _rigidBody.position - center;
            _boxCollider.edgeRadius = skinWidth;
            _skinWidth = skinWidth;
        }

        /* Immediately set facing of horizontal/vertical axes. */
        public void Flip(bool horizontal, bool vertical)
        {
            _rigidBody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidBody.transform.localEulerAngles = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);
            _rigidBody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _flippedHorizontal = horizontal;
            _flippedVertical   = vertical;
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
                Debug.DrawLine(_rigidBody.position, position, Color.grey, Time.fixedDeltaTime);
            }
            #endif
            _rigidBody.position = position;
        }

        /* Immediately move body by given amount. */
        public void MoveBy(Vector2 delta)
        {
            #if UNITY_EDITOR
            if (_drawMovesInEditor)
            {
                Debug.DrawLine(_rigidBody.position, _rigidBody.position + delta, Color.grey, Time.fixedDeltaTime);
            }
            #endif
            _rigidBody.position += delta;
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
            _rigidBody.position = startPositionThisFrame;
            _rigidBody.MovePosition(targetPositionThisFrame);
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
        Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width.

        If no layermask provided, uses the one assigned in editor.
        */
        public CollisionFlags2D CheckForOverlappingContacts(float extent)
        {
            Transform transform = _rigidBody.transform;
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
            if (_drawCastsInEditor)
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
        public bool ComputeOverlap(Collider2D collider, out Vector2 amount)
        {
            if (collider == null)
            {
                amount = Vector2.zero;
                return false;
            }

            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            Debug.Log(minimumSeparation.distance);
            if (!minimumSeparation.isValid || minimumSeparation.distance >= 0)
            {
                amount = Vector2.zero;
                return false;
            }

            amount = minimumSeparation.distance * minimumSeparation.normal;
            return true;
        }


        #if UNITY_EDITOR
        void OnValidate()
        {
            // force synchronization with inspector values whether game is playing in editor or not
            // note skip if bounds are empty, which occurs when a prefab is instantiated during an editor refresh
            if (_boxCollider != null && (_boxCollider.bounds.size.x != 0f || _boxCollider.bounds.size.y != 0f))
            {
                SetBounds(_boxCollider.bounds.min, _boxCollider.bounds.max, _skinWidth);
            }

            // update runtime data if inspector changed while game playing in editor
            if (Application.IsPlaying(this) && _initialized)
            {
                SetLayerMask(_layerMask);
                if (_preallocatedHitBufferSize != _hitBuffer.Length)
                {
                    _hitBuffer = new RaycastHit2D[_preallocatedHitBufferSize];
                }
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
        #endif
    }
}

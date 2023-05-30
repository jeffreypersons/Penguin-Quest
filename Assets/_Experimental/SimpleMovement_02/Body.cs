using System;
using UnityEngine;


namespace PQ._Experimental.SimpleMovement_002
{
    public class Body : MonoBehaviour
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

        public Vector2 Position  => _rigidBody.position;
        public float   Depth     => _rigidBody.transform.position.z;
        public Bounds  Bounds    => _boxCollider.bounds;
        public Vector2 Forward   => _rigidBody.transform.right.normalized;
        public Vector2 Up        => _rigidBody.transform.up.normalized;
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
            _hitBuffer  = new RaycastHit2D[_preallocatedHitBufferSize];
            _castFilter.useLayerMask = true;
            _castFilter.SetLayerMask(_layerMask);

            float buffer = Mathf.Clamp01(_skinWidth);
            _boxCollider.edgeRadius = buffer;
            _boxCollider.size       = new Vector2(1f - buffer, 1f - buffer);

            _rigidBody.isKinematic = true;
            _rigidBody.simulated   = true;
            _rigidBody.useFullKinematicContacts = true;
            _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

            _initialized = true;
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
        public bool CastAABB_All(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
        {
            _castFilter.SetLayerMask(_layerMask);

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
        
        /* Check each side for _any_ colliders occupying the region between AABB and the outer perimeter defined by skin width. */
        public CollisionFlags2D CheckForOverlappingContacts(float skinWidth)
        {
            Transform transform = _rigidBody.transform;
            Vector2 right = transform.right.normalized;
            Vector2 up    = transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            CollisionFlags2D flags = CollisionFlags2D.None;
            if (CastAABB_All(skinWidth * right, out _))
            {
                flags |= CollisionFlags2D.Front;
            }
            if (CastAABB_All(skinWidth * up, out _))
            {
                flags |= CollisionFlags2D.Above;
            }
            if (CastAABB_All(skinWidth * left, out _))
            {
                flags |= CollisionFlags2D.Behind;
            }
            if (CastAABB_All(skinWidth * down, out _))
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


        void OnValidate()
        {
            float buffer = Mathf.Clamp01(_skinWidth);
            _boxCollider.edgeRadius = buffer;
            _boxCollider.size       = new Vector2(1f - buffer, 1f - buffer);

            if (!Application.IsPlaying(this) || !_initialized)
            {
                return;
            }

            if (_preallocatedHitBufferSize != _hitBuffer.Length)
            {
                _hitBuffer = new RaycastHit2D[_preallocatedHitBufferSize];
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
using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEditor;
using PQ.Common.Extensions;


namespace PQ.Common.Physics
{
    /*
    Represents a physical body aligned with an AABB and driven by kinematic physics.

    This is intended to be our interface for collisions, casts, rigidbody movement.

    Notes
    * Assumes always upright bounding box, with kinematic rigidbody
    * Corresponding game object is fixed in rotation to enforce alignment with global up
    * Caching is done only for cast results, position caching is intentionally left to any calling code
    */
    [AddComponentMenu("KinematicBody2D")]
    public sealed class KinematicBody2D : MonoBehaviour
    {
        [Header("Thresholds")]

        [Tooltip("Attached rigidbody, used internally")]
        [SerializeField] private Rigidbody2D   _rigidBody;

        [Tooltip("Attached box collider, used internally as our object's primary AABB")]
        [SerializeField] private BoxCollider2D _boxCollider;

        [Tooltip("Layers to include in collision detection")]
        [SerializeField] private LayerMask _layerMask = default;


        [Header("Thresholds")]

        [Tooltip("Max degrees allowed for climbing a slope")]
        [SerializeField][Range(0, 90)] private float _maxAscendableSlopeAngle = 90f;

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] private float _overlapTolerance = 0.04f;


        [Header("Physical Properties")]

        [Tooltip("Scalar for reflection along the normal (bounciness is from 0 (no bounciness) to 1 (completely reflected))")]
        [SerializeField] [Range(0, 1)] private float _collisionBounciness = 0f;

        [Tooltip("Scalar for reflection along the tangent (friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance))")]
        [SerializeField] [Range(-1, 1)] private float _collisionFriction = 0f;

        [Tooltip("Multiplier for 2D gravity")]
        [SerializeField] [Range(0, 10)] private float _gravityScale = 1.00f;


        [Header("Advanced Physics Settings")]

        [Tooltip("Cap on number of movement solves (exposed only in editor, default suffices most the time)")]
        [SerializeField][Range(0, 50)] private int _maxSolverMoveIterations = 10;

        [Tooltip("Cap on number of overlap resolution solves (exposed only in editor, default suffices most the time)")]
        [SerializeField][Range(0, 50)] private int _maxSolverOverlapIterations = 2;

        [Tooltip("Size of buffer used to cache cast results (exposed only in editor, default suffices most the time)")]
        [SerializeField][Range(1, 100)] private int _preallocatedHitBufferSize = 16;

        
        #if UNITY_EDITOR
        [Flags]
        public enum EditorVisuals
        {
            None      = 0,
            Casts     = 1 << 1,
            Moves     = 1 << 2,
            Axes      = 1 << 3,
            Positions = 1 << 4,
            All       = ~0,
        }

        [Tooltip("Settings for easily toggling debug visuals in one place from the inspector")]
        [SerializeField] private EditorVisuals _editorVisuals = EditorVisuals.All;
        private bool IsEnabled(EditorVisuals flags) => (_editorVisuals & flags) == flags;
        #endif


        private bool _initialized;
        private bool _flippedHorizontal;
        private bool _flippedVertical;

        private ContactFilter2D   _castFilter;
        private RaycastHit2D[]    _hitBuffer;
        private KinematicSolver2D _solver;

        public override string ToString() =>
            $"KinematicBody2D{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB:bounds(center:{Center},extents:{Extents})," +
                $"Gravity:{Gravity}," +
                $"OverlapTolerance:{OverlapTolerance}," +
                $"Friction:{Friction}," +
                $"LayerMask:{LayerMask}," +
                $"MaxSolverMoveIterations:{MaxSolverMoveIterations}," +
                $"MaxSolverOverlapIterations:{MaxSolverOverlapIterations}," +
                $"PreallocatedHitBufferSize:{PreallocatedHitBufferSize}" +
            $"}}";

        /* Whether the body's relative x-axis is facing left or not. */
        public bool    FlippedHorizontal => _flippedHorizontal;

        /* Whether the body's relative x-axis is facing down or not. */
        public bool    FlippedVertical   => _flippedVertical;

        /* Position of body as anchored at the bottom center (not to be confused with AABB center position). */
        public Vector2 Position => _rigidBody.position;

        /* Center position of AABB. */
        public Vector2 Center => _boxCollider.bounds.center;

        /* Local forward direction. Always up unless horizontal flipped set to true. */
        public Vector2 Forward => _rigidBody.transform.right.normalized;

        /* Local upwards direction. Always up unless vertical flipped set to true. */
        public Vector2 Up      => _rigidBody.transform.up.normalized;
        
        /* Half size of bounding box (in other words, distance from AABB center to horizontal/vertical sides, including skinWidth). */
        public Vector2 Extents => _boxCollider.bounds.extents + new Vector3(_overlapTolerance, _overlapTolerance, 0f);

        /* Distance 'into' the screen. */
        public float Depth => _rigidBody.transform.position.z;

        /* Buffer amount measured from AABB into our bounding box, if any. This defines the acceptable overlap amount for collisions. */
        public float OverlapTolerance => _overlapTolerance;

        /* Gravity specific to the body, according to set scale. */
        public float Gravity => _gravityScale * - Mathf.Abs(Physics2D.gravity.y);

        /* Bounciness of material on collisions (bounciness is from 0 (no bounciness) to 1 (completely reflected)). */
        public float Bounciness => _collisionBounciness;

        /* Friction of material on collisions (friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance)). */
        public float Friction => _collisionFriction;

        /* Layer mask used when detecting collisions. */
        public LayerMask LayerMask => _castFilter.layerMask;

        /* Max degrees allowed for climbing a slope. */
        public float MaxAscendableSlopeAngle => _maxAscendableSlopeAngle;

        /* Cap on number of overlap resolution solves, used when solving for movement. */
        internal float MaxSolverMoveIterations => _maxSolverMoveIterations;

        /* Cap on number of movement resolution solves, used when resolving overlaps after solving for movement. */
        internal float MaxSolverOverlapIterations => _maxSolverOverlapIterations;

        /* Size of buffer used to cache cast results. */
        internal float PreallocatedHitBufferSize => _preallocatedHitBufferSize;


        [Pure]
        private static (Vector2 min, Vector2 max) GetOrientedLocalMinMaxCorners(BoxCollider2D box)
        {
            Vector2 xAxis = (box.bounds.extents.x + box.edgeRadius) * box.attachedRigidbody.transform.right.normalized;
            Vector2 yAxis = (box.bounds.extents.y + box.edgeRadius) * box.attachedRigidbody.transform.up.normalized;
            return (-xAxis - yAxis, xAxis + yAxis);
        }

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
            _castFilter.SetLayerMask(layerMask);
            _layerMask = layerMask;
        }

        /*
        Resize AABB to span between given local coordinates, with skin width as our collision contact offset.

        Note: Skin width is calculated _inwards_ from the given bound corners.
        */
        public void SetBounds(Vector2 from, Vector2 to, float overlapTolerance)
        {
            Vector2 localCenter = Vector2.LerpUnclamped(from, to, 0.50f);
            Vector2 localSize = new Vector2(
                x: Mathf.Abs(to.x - from.x) - (2f * overlapTolerance),
                y: Mathf.Abs(to.y - from.y) - (2f * overlapTolerance)
            );
            if (overlapTolerance < 0f || localSize.x <= 0 || localSize.y <= 0)
            {
                throw new ArgumentException(
                    $"Invalid bounds - expected 0 <= overlapTolerance < size={localSize}, " +
                    $"received from={from} to={to} overlapTolerance={overlapTolerance}");
            }

            _boxCollider.offset     = localCenter;
            _boxCollider.size       = localSize;
            _boxCollider.edgeRadius = overlapTolerance;
            _overlapTolerance       = overlapTolerance;
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

        public void Move(Vector2 delta)
        {
            _solver.SolveMovement(delta);
        }

        public bool IsContacting(CollisionFlags2D flags)
        {
            return _solver.InContact(flags);
        }

        /* Immediately move body to given point. */
        internal void MoveTo(Vector2 position)
        {
            #if UNITY_EDITOR
            if (IsEnabled(EditorVisuals.Moves))
            {
                Debug.DrawLine(_rigidBody.position, position, Color.grey, Time.fixedDeltaTime);
            }
            #endif
            _rigidBody.position = position;
        }

        /* Immediately move body by given amount. */
        internal void MoveBy(Vector2 delta)
        {
            #if UNITY_EDITOR
            if (IsEnabled(EditorVisuals.Moves))
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
            if (IsEnabled(EditorVisuals.Moves))
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
            if (IsEnabled(EditorVisuals.Casts))
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
            if (IsEnabled(EditorVisuals.Casts))
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
            // avoid updating with inspector if loading the original prefab from disk (which occurs before loading the instance)
            // otherwise the default inspector values are used. By skipping persistent objects, we effectively only update when values are
            // changed in the inspector
            if (EditorUtility.IsPersistent(this))
            {
                return;
            }

            (Vector2 localMin, Vector2 localMax) = GetOrientedLocalMinMaxCorners(_boxCollider);
            SetBounds(localMin, localMax, _overlapTolerance);

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
            Vector2 anchor  = _rigidBody.position;
            Vector2 center  = _boxCollider.bounds.center;
            Vector2 forward = _rigidBody.transform.right.normalized;
            Vector2 up      = _rigidBody.transform.up.normalized;
            Vector2 extents = _boxCollider.bounds.extents;
            Vector2 buffer  = new Vector2(_overlapTolerance, _overlapTolerance);

            if (IsEnabled(EditorVisuals.Positions))
            {
                GizmoExtensions.DrawSphere(anchor, 0.02f, Color.blue);
                GizmoExtensions.DrawSphere(center, 0.02f, Color.black);
            }
            if (IsEnabled(EditorVisuals.Axes))
            {
                Vector2 frontCenter = center + (extents.x + buffer.x) * forward;
                Vector2 topCenter   = center + (extents.y + buffer.y) * up;
                GizmoExtensions.DrawArrow(center, frontCenter, Color.red);
                GizmoExtensions.DrawArrow(center, topCenter,   Color.green);
                GizmoExtensions.DrawLine(frontCenter - buffer.x * forward, frontCenter, Color.black);
                GizmoExtensions.DrawLine(topCenter   - buffer.y * up,      topCenter,   Color.black);
            }
        }
        #endif
    }
}

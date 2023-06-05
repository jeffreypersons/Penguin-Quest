using System;
using UnityEngine;
using PQ.Common.Extensions;
using PQ.Common.Physics.Internal;


namespace PQ.Common.Physics
{
    /*
    Represents a physical body aligned with an AABB and driven by kinematic physics.

    This is intended to be our source of truth interface for dynamic rigidbody movement of kinematic entities.

    Notes
    * Assumes always upright bounding box, with kinematic rigidbody
    * Corresponding game object is fixed in rotation to enforce alignment with global up
    * Caching is done only for cast results, position caching is intentionally left to any calling code
    */
    [ExecuteAlways]
    [AddComponentMenu("PhysicsBody2D")]
    public sealed class PhysicsBody2D : MonoBehaviour
    {
        [Header("Components")]
        [Tooltip("Transform used for movement, expected to have attached rigidbody 2D and box collider 2D")]
        [SerializeField] private Transform _transform = default;


        [Header("Thresholds")]

        [Tooltip("Layers to include in collision detection")]
        [SerializeField] private LayerMask _layerMask = default;
        
        [Tooltip("Minimum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 _AABBCornerMin = -Vector2.one;

        [Tooltip("Maximum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 _AABBCornerMax =  Vector2.one;

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] private float _skinWidth = 0.04f;

        [Tooltip("Max degrees allowed for climbing a slope")]
        [SerializeField][Range(0, 90)] private float _maxAscendableSlopeAngle = 90f;


        [Header("Physical Properties")]

        [Tooltip("Scalar for reflection along the tangent (friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance))")]
        [SerializeField] [Range(-1, 1)] private float _collisionFriction = 0f;

        [Tooltip("Scalar for reflection along the normal (bounciness is from 0 (no bounciness) to 1 (completely reflected))")]
        [SerializeField][Range(0, 1)] private float _collisionBounciness = 0f;

        [Tooltip("Multiplier for 2D gravity")]
        [SerializeField] [Range(0, 10)] private float _gravityScale = 1.00f;


        [Header("Advanced Physics Settings")]

        [Tooltip("Cap on number of movement solves (exposed only in editor, default suffices most the time)")]
        [SerializeField][Range(0, 50)] private int _maxSolverMoveIterations = 10;

        [Tooltip("Cap on number of overlap resolution solves (exposed only in editor, default suffices most the time)")]
        [SerializeField][Range(0, 50)] private int _maxSolverContactAdjustmentIterations = 2;

        [Tooltip("Size of buffer(s) used to internally cache physics results (exposed only in editor, default suffices most the time)")]
        [SerializeField][Range(1, 100)] private int _preallocatedResultBufferSize = 16;

        
        private KinematicLinearSolver2D.Params _solverParams;
        private KinematicRigidbody2D    _kinematicBody;
        private KinematicLinearSolver2D _kinematicSolver;
        
        public LayerMask LayerMask => _kinematicBody.LayerMask;

        public float Gravity    => _kinematicBody.GravityScale * -Mathf.Abs(Physics2D.gravity.y);
        public float Friction   => _kinematicBody.Friction;
        public float Bounciness => _kinematicBody.Bounciness;

        public Vector2 Position  => _kinematicBody.Position;
        public Vector2 Center    => _kinematicBody.Center;
        public Vector2 Forward   => _kinematicBody.Forward;
        public Vector2 Up        => _kinematicBody.Up;
        public Vector2 Extents   => _kinematicBody.Extents;
        public float   Depth     => _kinematicBody.Depth;
        public float   SkinWidth => _kinematicBody.SkinWidth;


        public override string ToString() =>
            $"PhysicsBody2D{{" +
                $"Position:{_kinematicBody.Position}," +
                $"Depth:{_kinematicBody.Depth}," +
                $"Forward:{_kinematicBody.Forward}," +
                $"Up:{_kinematicBody.Up}," +
                $"SkinWidth:{_skinWidth}," +
                $"Friction:{_collisionFriction}," +
                $"Bounciness:{_collisionBounciness}," +
                $"LayerMask:{_layerMask}," +
                $"MaxSolverMoveIterations:{_maxSolverMoveIterations}," +
                $"MaxSolverOverlapIterations:{_maxSolverContactAdjustmentIterations}," +
                $"PreallocatedBufferSize:{_preallocatedResultBufferSize}" +
            $"}}";
        
        
        #if UNITY_EDITOR
        [Flags]
        public enum EditorVisuals
        {
            None       = 0,
            Axes       = 1 << 1,
            Positions  = 1 << 2,
            RayCasts   = 1 << 3,
            ShapeCasts = 1 << 4,
            Moves      = 1 << 5,
            All        = ~0,
        }

        [Tooltip("Settings for easily toggling debug visuals in one place from the inspector")]
        [SerializeField] private EditorVisuals _editorVisuals = EditorVisuals.All;
        private bool IsEnabled(EditorVisuals flags) => (_editorVisuals & flags) == flags;
        #endif


        private void Initialize(bool force)
        {
            if (force || _kinematicBody == null || !_kinematicBody.IsAttachedTo(_transform))
            {
                _solverParams    = new();
                _kinematicBody   = new KinematicRigidbody2D(_transform);
                _kinematicSolver = new KinematicLinearSolver2D(_kinematicBody, in _solverParams);
            }
        }

        private void SyncProperties()
        {
            _solverParams.MaxMoveIterations    = _maxSolverMoveIterations;
            _solverParams.MaxOverlapIterations = _maxSolverContactAdjustmentIterations;
            _solverParams.MaxSlopeAngle        = _maxAscendableSlopeAngle;

            SetLayerMask(_layerMask);
            SetAABBMinMax(_AABBCornerMin, _AABBCornerMax, _skinWidth);
            _kinematicBody.ResizeHitBuffer(_preallocatedResultBufferSize);
            _kinematicBody.SetPhysicalProperties(_collisionFriction, _collisionBounciness, _gravityScale);

            #if UNITY_EDITOR
            _solverParams.VisualizePath           = IsEnabled(EditorVisuals.Moves);
            _kinematicBody.DrawRayCastsInEditor   = IsEnabled(EditorVisuals.RayCasts);
            _kinematicBody.DrawShapeCastsInEditor = IsEnabled(EditorVisuals.ShapeCasts);
            #endif
        }


        void Awake()
        {
            Initialize(force: true);
            SyncProperties();
        }
        
        /* Set world transform to given point, ignoring physics. */
        public void TeleportTo(Vector2 position)
        {
            _kinematicBody.TeleportTo(position);
        }

        /* Immediately set facing of horizontal/vertical axes. */
        public void Move(Vector2 delta)
        {
            // note that we don't free position as that would disable the movement we 'schedule' and interpolate with
            _kinematicSolver.SolveMovement(delta);
        }

        /* Immediately set facing of horizontal/vertical axes. */
        public void Flip(bool horizontal, bool vertical)
        {
            // note that the constraints may need to be removed if we switch to doing interpolated rotation
            _kinematicBody.SetConstraints(RigidbodyConstraints2D.None);
            _kinematicBody.SetFlippedAmount(
                horizontalRatio: horizontal ? 1f : 0f,
                verticalRatio:   vertical   ? 1f : 0f
             );
            _kinematicBody.SetConstraints(RigidbodyConstraints2D.FreezeRotation);
        }


        /* Immediately set facing of horizontal/vertical axes. */
        public bool IsContacting(CollisionFlags2D flags)
        {
            return _kinematicSolver.InContact(flags);
        }


        /* Set layermask used for detecting collisions. */
        public void SetLayerMask(LayerMask layerMask)
        {
            _kinematicBody.SetLayerMask(layerMask);
            _layerMask = layerMask;
        }

        /*
        Resize bounding box to span between given local corners, with tolerance defining our bounds 'thickness'.

        Notes
        * Positions are relative to rigidbody position (eg anchor point at bottom center of sprite)
        * Size of box must be non zero and larger than twice our tolerance (ie amount of tolerance must be < 100%)
        */
        public void SetAABBMinMax(Vector2 localMin, Vector2 localMax, float skinWidth)
        {
            Vector2 localCenter = Vector2.LerpUnclamped(localMin, localMax, 0.50f);
            Vector2 localExtents = new Vector2(
                x: 0.50f * (localMax.x - localMin.x),
                y: 0.50f * (localMax.y - localMin.y)
            );
            if (skinWidth < 0f || localExtents.x <= 0 || localExtents.y <= 0)
            {
                throw new ArgumentException(
                    $"Invalid bounds - expected 0 <= skinWidth < extents={localExtents}, " +
                    $"received from={localMin} to={localMax} skinWidth={skinWidth}");
            }

            _kinematicBody.SetLocalBounds(localCenter, 2f * localExtents, skinWidth);
            _skinWidth     = skinWidth;
            _AABBCornerMin = localMin;
            _AABBCornerMax = localMax;
        }
        

        #if UNITY_EDITOR
        void OnValidate()
        {
            Initialize(force: false);
            SyncProperties();
        }

        void OnDrawGizmos()
        {
            Vector2 buffer = new Vector2(_skinWidth, _skinWidth);
            if (IsEnabled(EditorVisuals.Positions))
            {
                GizmoExtensions.DrawSphere(_kinematicBody.Position, 0.02f, Color.black);
                GizmoExtensions.DrawSphere(_kinematicBody.Center,   0.02f, Color.blue);
            }
            if (IsEnabled(EditorVisuals.Axes))
            {
                Vector2 frontCenter = _kinematicBody.Center + (_kinematicBody.Extents.x) * _kinematicBody.Forward;
                Vector2 topCenter   = _kinematicBody.Center + (_kinematicBody.Extents.y) * _kinematicBody.Up;

                GizmoExtensions.DrawArrow(_kinematicBody.Center, frontCenter, Color.red);
                GizmoExtensions.DrawArrow(_kinematicBody.Center, topCenter,   Color.green);

                GizmoExtensions.DrawLine(frontCenter - buffer.x * _kinematicBody.Forward, frontCenter, Color.black);
                GizmoExtensions.DrawLine(topCenter   - buffer.y * _kinematicBody.Up,      topCenter,   Color.black);
            }
        }
        #endif
    }
}

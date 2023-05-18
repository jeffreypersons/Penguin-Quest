using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEditor;
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
        [SerializeField][Range(0, 1)] private float _overlapTolerance = 0.04f;

        [Tooltip("Max degrees allowed for climbing a slope")]
        [SerializeField][Range(0, 90)] private float _maxAscendableSlopeAngle = 90f;


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

        private KinematicRigidbody2D    _body;
        private KinematicLinearSolver2D _solver;
        
        public Vector2 Position => _body.Position;
        public Vector2 Center   => _body.Center;
        public Vector2 Forward  => _body.Forward;
        public Vector2 Up       => _body.Up;
        public Vector2 Extents  => _body.Extents;
        public float   Depth    => _body.Depth;

        public float Gravity    => _gravityScale * -Mathf.Abs(Physics2D.gravity.y);
        public float Bounciness => _body.Bounciness;
        public float Friction   => _body.Friction;

        public LayerMask LayerMask => _body.LayerMask;
        public float OverlapTolerance => _body.OverlapTolerance;


        public override string ToString() =>
            $"PhysicsBody2D{{" +
                $"Position:{_body.Position}," +
                $"Depth:{_body.Depth}," +
                $"Forward:{_body.Forward}," +
                $"Up:{_body.Up}," +
                $"OverlapTolerance:{_overlapTolerance}," +
                $"Friction:{_collisionFriction}," +
                $"Bounciness:{_collisionBounciness}," +
                $"LayerMask:{_layerMask}," +
                $"MaxSolverMoveIterations:{_maxSolverMoveIterations}," +
                $"MaxSolverOverlapIterations:{_maxSolverOverlapIterations}," +
                $"PreallocatedHitBufferSize:{_preallocatedHitBufferSize}" +
            $"}}";


        void Awake()
        {
            SetLayerMask(_layerMask);
            SetAABBMinMax(_AABBCornerMin, _AABBCornerMax, _overlapTolerance);

            _body   = new KinematicRigidbody2D(transform);
            _solver = new KinematicLinearSolver2D(_body);
        }
        
        /* Set world transform to given point, ignoring physics. */
        public void TeleportTo(Vector2 position)
        {
            transform.position = position;
        }
        
        /* Immediately set facing of horizontal/vertical axes. */
        public void Flip(bool horizontal, bool vertical)
        {
            _body.SetFlippedAmount(
                horizontalRatio: horizontal ? 1f : 0f,
                verticalRatio:   vertical   ? 1f : 0f
             );
        }

        /* Immediately set facing of horizontal/vertical axes. */
        public void Move(Vector2 delta)
        {
            _solver.SolveMovement(delta);
        }

        public bool IsContacting(CollisionFlags2D flags)
        {
            return _solver.InContact(flags);
        }



        /* Set layermask used for detecting collisions. */
        public void SetLayerMask(LayerMask layerMask)
        {
            _layerMask = layerMask;
        }

        /*
        Resize bounding box to span between given local corners, with tolerance defining our bounds 'thickness'.

        Notes
        * Positions are relative to rigidbody position (eg anchor point at bottom center of sprite)
        * Size of box must be non zero and larger than twice our tolerance (ie amount of tolerance must be < 100%)
        */
        public void SetAABBMinMax(Vector2 localMin, Vector2 localMax, float overlapTolerance)
        {
            Vector2 localSize = new Vector2(
                x: Mathf.Abs(localMax.x - localMin.x) - (2f * overlapTolerance),
                y: Mathf.Abs(localMax.y - localMin.y) - (2f * overlapTolerance)
            );
            if (overlapTolerance < 0f || localSize.x <= 0 || localSize.y <= 0)
            {
                throw new ArgumentException(
                    $"Invalid bounds - expected 0 <= overlapTolerance < size={localSize}, " +
                    $"received from={localMin} to={localMax} overlapTolerance={overlapTolerance}");
            }

            _body.SetLocalBounds(localMin, localMax, overlapTolerance);
            _overlapTolerance = overlapTolerance;
            _AABBCornerMin    = localMin;
            _AABBCornerMax    = localMax;
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

            // if corners changed in editor, they take precedence over any manual changes to collider bounds
            if (!Mathf.Approximately(_overlapTolerance, _body.OverlapTolerance))
            {
                SetAABBMinMax(_body.Center - _body.Extents, _body.Center + _body.Extents, _overlapTolerance);
            }
            if (!Mathf.Approximately(_overlapTolerance, _body.OverlapTolerance))
            {
                SetAABBMinMax(_body.Center - _body.Extents, _body.Center + _body.Extents, _overlapTolerance);
            }

            // update runtime data if inspector changed while game playing in editor

            if (Application.IsPlaying(this))
            {
                SetLayerMask(_layerMask);
                _body.ResizeHitBuffer(_preallocatedHitBufferSize);
            }
        }


        void OnDrawGizmos()
        {
            Vector2 buffer  = new Vector2(_overlapTolerance, _overlapTolerance);

            if (IsEnabled(EditorVisuals.Positions))
            {
                GizmoExtensions.DrawSphere(_body.Position, 0.02f, Color.blue);
                GizmoExtensions.DrawSphere(_body.Center, 0.02f, Color.black);
            }
            if (IsEnabled(EditorVisuals.Axes))
            {
                Vector2 frontCenter = _body.Center + (_body.Extents.x + buffer.x) * _body.Forward;
                Vector2 topCenter   = _body.Center + (_body.Extents.y + buffer.y) * _body.Up;
                GizmoExtensions.DrawArrow(_body.Center, frontCenter, Color.red);
                GizmoExtensions.DrawArrow(_body.Center, topCenter,   Color.green);
                GizmoExtensions.DrawLine(frontCenter - buffer.x * _body.Forward, frontCenter, Color.black);
                GizmoExtensions.DrawLine(topCenter   - buffer.y * _body.Up,      topCenter,   Color.black);
            }
        }
        #endif
    }
}

using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private CharacterEntitySettings _settings;

        private Physics.SolverParams _solverParams;
        private GameplayInput _input;
        private ICharacterController2D _mover;

        private void SyncPropertiesFromSettings()
        {
            _solverParams = _solverParams with
            {
                MaxIterations   = _settings.solverIterationsPerPhysicsUpdate,

                Bounciness      = _settings.collisionBounciness,
                Friction        = _settings.collisionFriction,

                ContactOffset   = _settings.skinWidth,
                GroundLayerMask = _settings.groundLayerMask,
                MaxSlopeAngle   = _settings.maxAscendableSlopeAngle,
                Gravity         = _settings.gravityScale * Physics2D.gravity.y,
            };
        }


        private void Awake()
        {
            _solverParams = new Physics.SolverParams();
            SyncPropertiesFromSettings();
            _input = new GameplayInput();
            _mover = new SimpleCharacterController2D(gameObject, _solverParams);

        }


        void Update()
        {
            _input.ReadInput();
        }


        void FixedUpdate()
        {
            if (Mathf.Approximately(_input.Horizontal, 0f))
            {
                return;
            }

            bool characterMovingLeft = _input.Horizontal < 0;
            bool characterFacingLeft = _mover.Flipped;
            if (characterFacingLeft != characterMovingLeft)
            {
                _mover.Flip();
            }

            _mover.Move(new Vector2(_input.Horizontal * _settings.walkSpeed * Time.fixedDeltaTime, 0));
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_solverParams == null)
            {
                // only sync if solver params was already setup
                return;
            }
            SyncPropertiesFromSettings();
        }

        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // then draw a pair of arrows from the that should be identical to the transform's axes in the editor window
            Vector2 center = _mover.Bounds.center;
            Vector2 xAxis  = _mover.Forward * _mover.Bounds.extents.x;
            Vector2 yAxis  = _mover.Up      * _mover.Bounds.extents.y;
            float xOffsetRatio = 1f + _mover.ContactOffset / _mover.Bounds.extents.x;
            float yOffsetRatio = 1f + _mover.ContactOffset / _mover.Bounds.extents.y;

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
            GizmoExtensions.DrawRect(center, xOffsetRatio * xAxis, yOffsetRatio * yAxis, Color.magenta);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }
        #endif
    }
}

using System.Diagnostics.Contracts;
using UnityEngine;
using PQ.Common.Physics;


namespace PQ.Game.Entities
{
    public sealed class PenguinEntity
    {
        public System.Action OnGroundContactChanged { get; set; }
        public PenguinEntitySettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                if (!ReferenceEquals(_settings, value))
                {
                    return;
                }

                _settings = value;
                _settings.OnChanged = SyncPropertiesFromSettings;
                SyncPropertiesFromSettings();
            }
        }

        public float HorizontalInput { get => _horizontalInput; set => _horizontalInput = Mathf.Clamp01(value); }

        private PenguinEntitySettings _settings;
        private float _horizontalInput;
        private bool  _isGrounded;
        private float _walkSpeed;
        private float _jumpSpeed;
        private Vector2 _jumpDisplacementToPeak;
        private SolverParams _characterSolverParams;
        private ICharacterController2D _characterController;

        private void SyncPropertiesFromSettings()
        {
            _walkSpeed                           = Settings.walkSpeed;
            _jumpSpeed                           = Settings.jumpSpeed;
            _jumpDisplacementToPeak              = new Vector2(Settings.jumpLengthToApex, Settings.jumpHeightToApex);

            _characterSolverParams.MaxIterations = Settings.solverIterationsPerPhysicsUpdate;

            _characterSolverParams.Bounciness    = Settings.collisionBounciness;
            _characterSolverParams.Friction      = Settings.collisionFriction;

            _characterSolverParams.ContactOffset = Settings.skinWidth;
            _characterSolverParams.LayerMask     = Settings.groundLayerMask;
            _characterSolverParams.MaxSlopeAngle = Settings.maxAscendableSlopeAngle;
            _characterSolverParams.Gravity       = Mathf.Abs(Settings.gravityScale * Physics2D.gravity.y);

            Debug.Log($"Updated fields according to {Settings} {{" +
                $"WalkSpeed: {_walkSpeed}, " +
                $"JumpSpeed: {_jumpSpeed}, " +
                $"JumpDisplacementToPeak: {_jumpDisplacementToPeak}, " +
                $"SolverParams: {_characterSolverParams}}}");
        }


        public PenguinEntity(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<KinematicBody2D>(out var body))
            {
                throw new MissingComponentException($"Expected non-null {nameof(KinematicBody2D)}");
            }

            _horizontalInput       = 0f;
            _characterSolverParams = new SolverParams();
            _characterController   = new SimpleCharacterController2D(gameObject, _characterSolverParams);
        }

        public void UpdatePhysics()
        {
            Vector2 velocity = Vector2.zero;
            if (RequestedMoveInOppositeDirection(HorizontalInput, _characterController))
            {
                _characterController.Flip();
            }

            if (!Mathf.Approximately(HorizontalInput, 0f))
            {
                velocity.x += _horizontalInput * _walkSpeed;
            }


            if (_isGrounded != _characterController.IsGrounded)
            {
                _isGrounded = _characterController.IsGrounded;
                OnGroundContactChanged?.Invoke();
            }
                        
            if (!_characterController.IsGrounded)
            {
                // todo: move this stuff into the solver, so it can do things like faster steep-slope-sliding
                velocity.y -= _characterSolverParams.Gravity;
            }

            _characterController.Move(Time.fixedDeltaTime * velocity);
        }


        [Pure]
        private static bool RequestedMoveInOppositeDirection(float horizontalInput, ICharacterController2D controller)
        {
            if (Mathf.Approximately(horizontalInput, 0f))
            {
                return false;
            }
            bool characterMovingLeft = horizontalInput < 0;
            bool characterFacingLeft = controller.Flipped;
            return characterFacingLeft != characterMovingLeft;
        }
    }
}

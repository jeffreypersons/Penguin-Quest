using System.Diagnostics.Contracts;
using UnityEngine;
using PQ.Common.Physics;


namespace PQ.Game.Entities
{
    public class PenguinEntity : MonoBehaviour
    {
        public System.Action OnGroundInputChanged { get; set; }
        public PenguinEntitySettings Settings { get; set; }

        private bool  _isGrounded;
        private float _walkSpeed;
        private float _jumpSpeed;
        private Vector2 _jumpDisplacementToPeak;
        private SolverParams _characterSolverParams;
        private ICharacterController2D _characterController;
        private GameplayInput _characterInput;

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


        private void Awake()
        {
            _characterInput        = new GameplayInput();
            _characterSolverParams = new SolverParams();
            _characterController   = new SimpleCharacterController2D(gameObject, _characterSolverParams);
        }

        private void Start()
        {
            if (Settings == null)
            {
                throw new MissingComponentException($"Settings required - " +
                    $"no instance of {Settings.name} found attached to {gameObject.name}");
            }

            Settings.OnChanged = SyncPropertiesFromSettings;
            SyncPropertiesFromSettings();
        }

        void Update()
        {
            _characterInput.ReadInput();
        }


        void FixedUpdate()
        {
            Vector2 velocity = Vector2.zero;
            if (RequestedMoveInOppositeDirection(_characterInput, _characterController))
            {
                _characterController.Flip();
            }

            if (!Mathf.Approximately(_characterInput.Horizontal, 0f))
            {
                velocity.x += _characterInput.Horizontal * _walkSpeed;
            }


            if (_isGrounded != _characterController.IsGrounded)
            {
                _isGrounded = _characterController.IsGrounded;
                OnGroundInputChanged?.Invoke();
            }

            if (_characterController.IsGrounded && _characterInput.Vertical > 0f)
            {
                // todo: replace with 'real' jump calculations
                velocity.y += _characterInput.Vertical * _jumpSpeed;
            }
            
            if (!_characterController.IsGrounded)
            {
                // todo: move this stuff into the solver, so it can do things like faster steep-slope-sliding
                velocity.y -= _characterSolverParams.Gravity;
            }

            //_characterController.Move(Time.fixedDeltaTime * velocity);
        }


        [Pure]
        private static bool RequestedMoveInOppositeDirection(
            GameplayInput input, ICharacterController2D controller)
        {
            if (Mathf.Approximately(input.Horizontal, 0f))
            {
                return false;
            }
            bool characterMovingLeft = input.Horizontal < 0;
            bool characterFacingLeft = controller.Flipped;
            return characterFacingLeft != characterMovingLeft;
        }
    }
}

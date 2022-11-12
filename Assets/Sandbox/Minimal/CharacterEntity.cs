using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private CharacterEntitySettings _settings;

        private Vector2 _walkVelocity;
        private Vector2 _jumpDisplacementToPeak;
        private Physics.SolverParams _characterSolverParams;

        private GameplayInput _characterInput;
        private ICharacterController2D _characterController;

        private void SyncPropertiesFromSettings()
        {
            _walkVelocity                        = new Vector2(_settings.walkSpeed, 0);
            _jumpDisplacementToPeak              = new Vector2(_settings.jumpLengthToApex, _settings.jumpHeightToApex);
            _characterSolverParams.MaxIterations = _settings.solverIterationsPerPhysicsUpdate;
            _characterSolverParams.Bounciness    = _settings.collisionBounciness;
            _characterSolverParams.Friction      = _settings.collisionFriction;
            _characterSolverParams.ContactOffset = _settings.skinWidth;
            _characterSolverParams.LayerMask     = _settings.groundLayerMask;
            _characterSolverParams.MaxSlopeAngle = _settings.maxAscendableSlopeAngle;
            _characterSolverParams.Gravity       = _settings.gravityScale * Physics2D.gravity.y;

            Debug.Log($"Updated fields according to {_settings} {{" +
                $"WalkVelocity: {_walkVelocity}, " +
                $"JumpDisplacementToPeak: {_jumpDisplacementToPeak}, " +
                $"SolverParams: {_characterSolverParams}}}");
        }


        private void Awake()
        {
            if (_settings == null)
            {
                throw new MissingComponentException($"Settings required - " +
                    $"no instance of {_settings.name} found attached to {gameObject.name}");
            }

            _characterSolverParams = new Physics.SolverParams();
            _characterInput        = new GameplayInput();
            _characterController   = new SimpleCharacterController2D(gameObject, _characterSolverParams);
            _settings.OnChanged = SyncPropertiesFromSettings;
            SyncPropertiesFromSettings();
        }


        void Update()
        {
            _characterInput.ReadInput();
        }


        void FixedUpdate()
        {
            if (Mathf.Approximately(_characterInput.Horizontal, 0f))
            {
                return;
            }

            bool characterMovingLeft = _characterInput.Horizontal < 0;
            bool characterFacingLeft = _characterController.Flipped;
            if (characterFacingLeft != characterMovingLeft)
            {
                _characterController.Flip();
            }

            _characterController.Move(new Vector2(_characterInput.Horizontal * _settings.walkSpeed * Time.fixedDeltaTime, 0));
        }
    }
}

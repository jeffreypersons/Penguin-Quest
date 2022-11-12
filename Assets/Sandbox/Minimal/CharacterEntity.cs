using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private CharacterEntitySettings _settings;

        private float _walkSpeed;
        private Vector2 _jumpDisplacementToPeak;
        private Physics.SolverParams _characterSolverParams;

        private GameplayInput _characterInput;
        private ICharacterController2D _characterController;

        private void SyncPropertiesFromSettings()
        {
            _walkSpeed                          = _settings.walkSpeed;
            _jumpDisplacementToPeak             = new Vector2(_settings.jumpLengthToApex, _settings.jumpHeightToApex);

            _characterSolverParams.MaxIterations = _settings.solverIterationsPerPhysicsUpdate;

            _characterSolverParams.Bounciness    = _settings.collisionBounciness;
            _characterSolverParams.Friction      = _settings.collisionFriction;

            _characterSolverParams.ContactOffset = _settings.skinWidth;
            _characterSolverParams.LayerMask     = _settings.groundLayerMask;
            _characterSolverParams.MaxSlopeAngle = _settings.maxAscendableSlopeAngle;
            _characterSolverParams.Gravity       = _settings.gravityScale * Physics2D.gravity.y;

            Debug.Log($"Updated fields according to {_settings} {{" +
                $"WalkVelocity: {_walkSpeed}, " +
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
            Vector2 currentWalkVelocity = new (_characterInput.Horizontal * _walkSpeed, 0f);

            if (RequestedMoveInOppositeDirection(_characterInput, _characterController))
            {
                _characterController.Flip();
            }


            Vector2 totalVelocity = currentWalkVelocity;
            _characterController.Move(Time.fixedDeltaTime * totalVelocity);
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

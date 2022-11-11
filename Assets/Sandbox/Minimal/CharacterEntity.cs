using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private CharacterEntitySettings _settings;

        private Physics.SolverParams   _solverParams;
        private GameplayInput          _input;
        private ICharacterController2D _characterController;

        private void SyncPropertiesFromSettings()
        {
            _solverParams = _solverParams with
            {
                MaxIterations = _settings.solverIterationsPerPhysicsUpdate,

                Bounciness    = _settings.collisionBounciness,
                Friction      = _settings.collisionFriction,

                ContactOffset = _settings.skinWidth,
                LayerMask     = _settings.groundLayerMask,
                MaxSlopeAngle = _settings.maxAscendableSlopeAngle,
                Gravity       = _settings.gravityScale * Physics2D.gravity.y,
            };

        }


        private void Awake()
        {
            _solverParams = new Physics.SolverParams();
            SyncPropertiesFromSettings();
            _input = new GameplayInput();

            _characterController = new SimpleCharacterController2D(gameObject, _solverParams);
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
            bool characterFacingLeft = _characterController.Flipped;
            if (characterFacingLeft != characterMovingLeft)
            {
                _characterController.Flip();
            }

            _characterController.Move(new Vector2(_input.Horizontal * _settings.walkSpeed * Time.fixedDeltaTime, 0));
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
        #endif
    }
}

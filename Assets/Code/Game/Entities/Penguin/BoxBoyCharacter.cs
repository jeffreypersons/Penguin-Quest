using PQ.Common.Physics;
using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.SimpleMovement
{
    public class BoxBoyCharacter : MonoBehaviour
    {
        [Range(0, 10)] [SerializeField] private float _timeScale            = 1f;
        [Range(0, 10)] [SerializeField] private float _horizontalSpeed      = 5f;
        [Range(0, 50)] [SerializeField] private float _gravitySpeed         = 10f;
        [Range(0, 90)] [SerializeField] private float _maxSlopeAngle        = 90f;
        [Range(0, 50)] [SerializeField] private int   _maxMoveIterations    = 10;        
        [Range(0, 10)] [SerializeField] private int   _maxOverlapIterations = 2;


        private SolverParams _characterSolverParams = new();
        private void SyncPropertiesFromSettings()
        {
            _characterSolverParams.MaxMoveIterations = _maxMoveIterations;

            _characterSolverParams.Bounciness = 0f;
            _characterSolverParams.Friction = 0f;

            _characterSolverParams.MaxSlopeAngle = _maxSlopeAngle;
            _characterSolverParams.Gravity = Mathf.Abs(_gravitySpeed);

            Debug.Log($"Updated fields {{" +
                $"WalkSpeed: {_horizontalSpeed}, " +
                $"SolverParams: {_characterSolverParams}}}");
        }

        private bool    _grounded  = true;
        private Vector2 _inputAxis = Vector2.zero;
        private CollideAndSlideSolver2D _mover;

        public override string ToString() =>
            $"Character{{" +
                $"horizontalSpeed:{_horizontalSpeed}," +
                $"gravitySpeed:{_gravitySpeed}," +
                $"maxMoveIterations:{_maxMoveIterations}," +
                $"maxOverlapIterations:{_maxOverlapIterations}" +
            $"}}";

        
        private void Awake()
        {
            // set fps to 60 for more determinism when testing movement
            Application.targetFrameRate = 60;

            _mover = new CollideAndSlideSolver2D(gameObject.GetComponent<KinematicBody2D>(), in _characterSolverParams);
        }

        void Update()
        {
            SyncPropertiesFromSettings();
            _inputAxis = new(
                x: (Keyboard.current[Key.A].isPressed ? -1f : 0f) + (Keyboard.current[Key.D].isPressed ? 1f : 0f),
                y: (Keyboard.current[Key.S].isPressed ? -1f : 0f) + (Keyboard.current[Key.W].isPressed ? 1f : 0f)
            );

            Time.timeScale = _timeScale;
        }

        void FixedUpdate()
        {
            if (!Mathf.Approximately(_inputAxis.x, 0f))
            {
                _mover.Flip(horizontal: _inputAxis.x < 0);
            }

            float time = Time.fixedDeltaTime;
            Vector2 velocity = new(
                x: _inputAxis.x * _horizontalSpeed,
                y: _grounded? 0 : -_gravitySpeed
            );

            _mover.Move(time * velocity);
            _grounded = _mover.InContact(CollisionFlags2D.Below);
        }
    }
}

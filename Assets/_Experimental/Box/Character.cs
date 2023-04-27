using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ.TestScenes.Box
{
    public class Character : MonoBehaviour
    {
        [Range(0, 10)] [SerializeField] private float _horizontalSpeed     = 5f;
        [Range(0, 50)] [SerializeField] private float _gravitySpeed        = 10f;
        [Range(0, 90)] [SerializeField] private float _maxSlopeAngle       = 90f;
        [Range(0, 50)] [SerializeField] private int   _maxSolverIterations = 10;

        private bool    _grounded  = true;
        private Vector2 _inputAxis = Vector2.zero;
        private Mover   _mover;

        public override string ToString() =>
            $"Character{{" +
                $"horizontalSpeed:{_horizontalSpeed}," +
                $"gravitySpeed:{_gravitySpeed}," +
                $"maxSolverIterations:{_maxSolverIterations}," +
            $"}}";

        
        private void Awake()
        {
            _mover = new Mover(gameObject.transform);
        }

        void Update()
        {
            _inputAxis = new(
                x: (Keyboard.current[Key.A].isPressed ? -1f : 0f) + (Keyboard.current[Key.D].isPressed ? 1f : 0f),
                y: (Keyboard.current[Key.S].isPressed ? -1f : 0f) + (Keyboard.current[Key.W].isPressed ? 1f : 0f)
            );

            _mover.SetParams(_maxSlopeAngle, _maxSolverIterations);
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

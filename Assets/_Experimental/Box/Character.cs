using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ.TestScenes.Box
{
    public class Character : MonoBehaviour
    {
        [Range(0, 10)][SerializeField] private float _horizontalSpeed     = 20f;        
        [Range(0, 50)][SerializeField] private float _gravitySpeed        = 10f;
        [Range(0, 50)][SerializeField] private int   _maxSolverIterations = 10;
        [Range(0,  1)][SerializeField] private float _skinWidth           = 0.25f;

        private bool    _grounded  = false;
        private Vector2 _inputAxis = Vector2.zero;
        private Mover   _mover;

        public override string ToString() =>
            $"Character{{" +
                $"horizontalSpeed:{_horizontalSpeed}," +
                $"gravitySpeed:{_gravitySpeed}," +
                $"maxSolverIterations:{_maxSolverIterations}," +
                $"skinWidth:{_skinWidth}" +
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
        }

        void FixedUpdate()
        {
            if (!Mathf.Approximately(_inputAxis.x, 0f))
            {
                _mover.Flip(horizontal: _inputAxis.x < 0, vertical: false);
            }

            float time = Time.fixedDeltaTime;
            Vector2 velocity = new(
                x: _inputAxis.x * _horizontalSpeed,
                y: -_gravitySpeed
            );

            _mover.Move(time * velocity);
        }
    }
}

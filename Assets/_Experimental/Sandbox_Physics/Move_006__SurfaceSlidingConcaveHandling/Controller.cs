using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Physics.Move_006
{
    public class Controller : MonoBehaviour
    {
        [Range(0,  10)][SerializeField] private float _timeScale = 1f;
        [Range(0, 100)][SerializeField] private float _moveSpeed = 5f;

        private Vector2 _inputAxis;

        private KinematicBody2D         _kinematicBody;
        private KinematicLinearSolver2D _kinematicSolver;
        private CircularBuffer<Vector2> _positionHistory;


        void Awake()
        {
            Application.targetFrameRate = 60;

            _kinematicBody   = new KinematicBody2D(transform);
            _kinematicSolver = new KinematicLinearSolver2D(_kinematicBody);
            _positionHistory = new CircularBuffer<Vector2>(capacity: 50);
        }

        void Update()
        {
            if (!Mathf.Approximately(Time.timeScale, _timeScale))
            {
                Time.timeScale = _timeScale;
            }
            _inputAxis = new Vector2(
                x: (Keyboard.current[Key.A].isPressed ? -1f : 0f) + (Keyboard.current[Key.D].isPressed ? 1f : 0f),
                y: (Keyboard.current[Key.S].isPressed ? -1f : 0f) + (Keyboard.current[Key.W].isPressed ? 1f : 0f)
            );
        }

        void FixedUpdate()
        {
            Vector2 position = _kinematicBody.Position;
            if (_positionHistory.IsEmpty || _positionHistory.Back != position)
            {
                _positionHistory.PushBack(_kinematicBody.Position);
            }

            if (!Mathf.Approximately(_inputAxis.x, 0f))
            {
                _kinematicSolver.Flip(horizontal: _inputAxis.x < 0, vertical: false);
            }

            Vector2 deltaPosition = Time.fixedDeltaTime * _moveSpeed * _inputAxis;
            _kinematicSolver.Move(deltaPosition);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            _kinematicSolver.RemoveOverlap(collision.collider);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            _kinematicSolver.RemoveOverlap(collision.collider);
        }

        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || _positionHistory.Size < 2)
            {
                return;
            }

            for (int i = 1; i < _positionHistory.Size; i++)
            {
                GizmoExtensions.DrawArrow(_positionHistory[i - 1], _positionHistory[i], Color.cyan);
            }
        }
    }
}

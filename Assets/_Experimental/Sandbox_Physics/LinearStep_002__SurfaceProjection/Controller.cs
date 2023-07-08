using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Physics.LinearStep_002
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Collider2D _target;
        
        private bool _nextButtonPressed;
        private KinematicBody2D _kinematicBody;
        private KinematicLinearSolver2D _kinematicSolver;
        private CircularBuffer<Vector2> _positionHistory;


        void Awake()
        {
            Application.targetFrameRate = 60;
            _nextButtonPressed = false;
            _kinematicBody     = new KinematicBody2D(_transform);
            _kinematicSolver   = new KinematicLinearSolver2D(_kinematicBody);
            _positionHistory   = new CircularBuffer<Vector2>(capacity: 50);
        }

        void Update()
        {
            _nextButtonPressed = Keyboard.current[Key.Space].wasPressedThisFrame;
        }

        void FixedUpdate()
        {
            Vector2 position = _kinematicBody.Position;
            if (_positionHistory.IsEmpty || _positionHistory.Back != position)
            {
                _positionHistory.PushBack(_kinematicBody.Position);
            }

            if (_nextButtonPressed)
            {
                _kinematicSolver.MoveUnobstructedAlongDelta((Vector2)_target.bounds.center - position);
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || _positionHistory.Size < 2)
            {
                return;
            }

            for (int i = 1; i < _positionHistory.Size; i++)
            {
                GizmoExtensions.DrawArrow(_positionHistory[i-1], _positionHistory[i], Color.cyan);
            }
        }
    }
}

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
            _positionHistory   = new CircularBuffer<Vector2>(capacity: 100);
        }

        void Update()
        {
            _nextButtonPressed = Keyboard.current[Key.Space].wasPressedThisFrame;
        }

        void FixedUpdate()
        {
            if (_nextButtonPressed)
            {
                _positionHistory.PushBack(_kinematicBody.Position);
                _kinematicSolver.MoveUnobstructedAlongDelta((Vector2)_target.bounds.center - _kinematicBody.Position);
            }
        }

        void OnDrawGizmos()
        {
            if (_positionHistory.Size < 2)
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

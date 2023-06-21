using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_005
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Collider2D _target;
        [SerializeField][Range(0, 100)] private int _maxMinSeparationSolves = 10;

        private Internal.KinematicBody2D _kinematicBody;
        private Internal.KinematicLinearSolver2D _kinematicSolver;
        private bool _nextButtonPressed;


        void Awake()
        {
            Application.targetFrameRate = 60;
            _nextButtonPressed = false;

            _kinematicBody   = new Internal.KinematicBody2D(_transform);
            _kinematicSolver = new Internal.KinematicLinearSolver2D(_kinematicBody);
        }

        void Update()
        {
            _nextButtonPressed = Keyboard.current[Key.Space].wasPressedThisFrame;
        }

        void FixedUpdate()
        {
            if (_nextButtonPressed)
            {
                _kinematicSolver.MoveUnobstructedAlongDelta((Vector2)_target.bounds.center - _kinematicBody.Position);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ.TestScenes.Box
{
    public class Character : MonoBehaviour
    {
        [Range(0, 10)][SerializeField] private float _horizontalSpeed     = 20f;        
        [Range(0, 50)][SerializeField] private float _gravitySpeed        = 10f;
        [Range(0, 50)][SerializeField] private int   _maxSolverIterations = 10;
        [Range(0,  1)][SerializeField] private float _skinWidth           = 0.025f;

        private bool    _grounded  = true;
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

            _mover.SetSkinWidth(_maxSolverIterations);
            _mover.SetMaxSolverIterations(_maxSolverIterations);
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
                y: _grounded? 0 : -_gravitySpeed
            );

            _mover.Move(time * velocity);
            _grounded = _mover.InContact(CollisionFlags2D.Below);
        }


        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // surrounded by an outer bounding box offset by our skin with, with a pair of arrows from the that
            // should be identical to the transform's axes in the editor window
            Bounds box = _mover.Bounds;
            Vector2 center    = new(box.center.x, box.center.y);
            Vector2 skinRatio = new(1f + (_skinWidth / box.extents.x), 1f + (_skinWidth / box.extents.y));
            Vector2 xAxis     = box.extents.x * _mover.Forward;
            Vector2 yAxis     = box.extents.y * _mover.Up;

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
            GizmoExtensions.DrawRect(center, skinRatio.x * xAxis, skinRatio.y * yAxis, Color.magenta);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }
    }
}

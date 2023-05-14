using UnityEngine;
using PQ.Common.Physics;


namespace PQ.Game.Entities
{
    /*
    Movement component for applying given input and hooking together other physics components.
    */
    public sealed class MovementController2D
    {
        private KinematicBody2D _body;

        private bool _grounded;

        public MovementController2D(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<KinematicBody2D>(out var body))
            {
                throw new MissingComponentException($"Expected non-null {nameof(KinematicBody2D)}");
            }

            _body = body;
            _grounded = _body.IsContacting(CollisionFlags2D.Below);
        }

        public bool IsGrounded { get; private set; }

        // todo: find better way to encapsulate this
        public KinematicBody2DSettings Settings { get => _body.Settings; set => _body.Settings = value; }


        public void Move(Vector2 inputAxis, float maxHorizontalSpeed, float time)
        {
            if (!Mathf.Approximately(inputAxis.x, 0f))
            {
                _body.Flip(horizontal: inputAxis.x < 0, vertical: false);
            }

            // todo: check inputAxis.y for jumps

            Vector2 velocity = new(
                x: maxHorizontalSpeed * inputAxis.x,
                y: _grounded? 0 : _body.Gravity
            );

            _body.Move(time * velocity);
            _grounded = _body.IsContacting(CollisionFlags2D.Below);
        }
    }
}

using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState<PenguinStateId, PenguinEntity>
    {
        private Vector2 _velocity;
        private bool _wasGrounded;

        public PenguinStateMidair() : base() { }

        protected override void OnInitialize()
        {
            // ground contact is polled in OnFixedUpdate
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.JumpUp);
            _velocity = new Vector2(0f, Blob.Config.jumpImpulse);
            _wasGrounded = false;
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            _velocity.y += Blob.PhysicsBody.Gravity * Time.fixedDeltaTime;
            _velocity.y = Mathf.Clamp(_velocity.y, -Blob.Config.maxVerticalSpeedFalling, Blob.Config.maxVerticalSpeedJumping);

            Blob.PhysicsBody.Move(_velocity * Time.fixedDeltaTime);

            bool isGrounded = Blob.IsGrounded;
            if (!_wasGrounded && isGrounded)
            {
                base.SignalMoveToPreviousState();
            }
            _wasGrounded = isGrounded;
        }
    }
}

using UnityEngine;
using PQ.Common.Fsm;
using PQ.Common.Physics;


namespace PQ.Game.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState<PenguinStateId, PenguinEntity>
    {
        private bool _wasGrounded;

        public PenguinStateMidair() : base() { }

        protected override void OnInitialize()
        {
            // ground contact is polled in OnFixedUpdate
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.JumpUp);
            Blob.Animation.SetBool(PenguinAnimationParamId.IsGrounded, false);
            _wasGrounded = false;
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            bool isGrounded = Blob.PhysicsBody.IsContacting(CollisionFlags2D.Below);
            Blob.Animation.SetBool(PenguinAnimationParamId.IsGrounded, isGrounded);

            if (!_wasGrounded && isGrounded)
            {
                base.SignalMoveToPreviousState();
            }
            _wasGrounded = isGrounded;
        }
    }
}

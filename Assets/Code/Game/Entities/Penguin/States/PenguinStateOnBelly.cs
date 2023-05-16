using UnityEngine;
using PQ.Common.Fsm;
using PQ.Common.Physics;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinStateId, PenguinEntity>
    {
        private bool _grounded;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnBelly() : base() { }


        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.standUpCommand,      HandleStandUpInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
        }

        protected override void OnEnter()
        {
            _horizontalInput = new(HorizontalInput.Type.None);

            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            Blob.Skeleton.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;

            Blob.PhysicsBody.SetBounds(Blob.Config.boundsMinProne, Blob.Config.boundsMaxProne, Blob.Config.overlapToleranceProne);
            _grounded = Blob.PhysicsBody.IsContacting(CollisionFlags2D.Below);
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            if (!Mathf.Approximately(_horizontalInput.value, 0f))
            {
                Blob.PhysicsBody.Flip(horizontal: _horizontalInput.value < 0, vertical: false);
            }

            // todo: check inputAxis.y for jumps

            Vector2 velocity = new(
                x: Blob.Config.maxHorizontalSpeedUpright * _horizontalInput.value,
                y: _grounded ? 0 : Blob.PhysicsBody.Gravity
            );

            _grounded = Blob.PhysicsBody.IsContacting(CollisionFlags2D.Below);

            Blob.PhysicsBody.Move(velocity * Time.fixedDeltaTime);
        }

        protected override void OnUpdate()
        {
            // no op
        }


        private void HandleStandUpInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.StandingUp);
        }

        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
        }
    }
}

using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinStateId, PenguinEntity>
    {
        private HorizontalInput _horizontalInput;

        public PenguinStateOnBelly() : base() { }


        protected override void OnInitialize()
        {
            RegisterEvent(Blob.EventBus.standUpCommand,      HandleStandUpInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
            RegisterEvent(Blob.Config.OnChangedInEditor,     HandleConfigChanged);
        }

        protected override void OnEnter()
        {
            // turn off feet and flippers, since they overlap when sliding around
            _horizontalInput = new(HorizontalInput.Type.None);
            Blob.Skeleton.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;
            HandleConfigChanged();
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

            Vector2 velocity = new(
                x: Blob.Config.maxHorizontalSpeedUpright * _horizontalInput.value,
                y: Blob.IsGrounded ? 0 : Blob.PhysicsBody.Gravity
            );

            Blob.PhysicsBody.Move(velocity * Time.fixedDeltaTime);
        }

        protected override void OnUpdate()
        {
            // no op
        }
        
        private void HandleConfigChanged()
        {
            Blob.PhysicsBody.SetAABBMinMax(Blob.Config.boundsMinProne, Blob.Config.boundsMaxProne, Blob.Config.skinWidthProne);
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

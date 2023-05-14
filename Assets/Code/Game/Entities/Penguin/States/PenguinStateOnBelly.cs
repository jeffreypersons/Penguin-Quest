using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinStateId, PenguinEntity>
    {
        public PenguinStateOnBelly() : base() { }

        private HorizontalInput _horizontalInput;

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.standUpCommand,      HandleStandUpInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
        }

        protected override void OnEnter()
        {
            Blob.Movement.Settings = Blob.BellySettings;
            _horizontalInput = new(HorizontalInput.Type.None);

            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            Blob.Skeleton.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;

            // todo: might want to be more explicit that this causes a bounding box resize...
            Blob.Movement.Settings = Blob.BellySettings;
        }

        protected override void OnExit()
        {
            Blob.Movement.Move(new Vector2(_horizontalInput.value, 0f), Blob.MaxWalkSpeed, Time.fixedDeltaTime);
        }

        protected override void OnUpdate()
        {
            HandleHorizontalMovement();
        }

        private void HandleStandUpInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.StandingUp);
        }

        // todo: find a flexible solution for all this duplicated movement code in multiple states
        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            // no op
        }

        private void HandleHorizontalMovement()
        {
            // no op
        }
    }
}

using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState<PenguinStateId, PenguinEntity>
    {
        public PenguinStateOnFeet() : base() { }

        private HorizontalInput _horizontalInput;

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.lieDownCommand,      HandleLieDownInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
        }

        protected override void OnEnter()
        {
            Blob.Movement.Settings = Blob.FeetSettings;
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        protected override void OnExit()
        {
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        protected override void OnFixedUpdate()
        {
            Blob.Movement.Move(new Vector2(_horizontalInput.value, 0f), Blob.MaxWalkSpeed, Time.fixedDeltaTime);
        }

        protected override void OnUpdate()
        {

        }


        private void HandleLieDownInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.LyingDown);
        }

        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
        }
    }
}

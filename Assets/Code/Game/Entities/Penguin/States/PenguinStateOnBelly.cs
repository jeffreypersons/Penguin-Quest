using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinStateId, PenguinFsmSharedData>
    {
        public PenguinStateOnBelly() : base() { }

        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.standUpCommand,                    HandleStandUpInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange,               HandleMoveHorizontalChanged);
            //RegisterEvent(Blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged); // disabled until we fix ground handling
        }

        protected override void OnEnter()
        {
            Blob.CharacterController.Settings = Blob.OnBellySettings;
            _locomotionBlend = 0.0f;
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        protected override void OnExit()
        {
            _locomotionBlend = 0.0f;
            //Blob.Animation.SetFloat(PenguinAnimationParamId.LocomotionIntensity, _locomotionBlend);
        }

        protected override void OnUpdate()
        {
            HandleHorizontalMovement();
        }


        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void HandleGroundContactChanged(bool isGrounded)
        {
            Blob.Animation.SetBool(PenguinAnimationParamId.IsGrounded, isGrounded);
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

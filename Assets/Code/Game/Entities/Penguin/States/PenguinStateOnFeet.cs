using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState<PenguinStateId, PenguinFsmSharedData>
    {
        public PenguinStateOnFeet() : base() { }

        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.lieDownCommand,      HandleLieDownInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
        }

        protected override void OnEnter()
        {
            Blob.CharacterController.Settings = Blob.OnFeetSettings;
            _locomotionBlend = 0.0f;
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        protected override void OnExit()
        {
            _locomotionBlend = 0.0f;
            _horizontalInput = new(HorizontalInput.Type.None);
            //Blob.Animation.SetFloat(PenguinAnimationParamId.LocomotionIntensity, _locomotionBlend);
        }

        protected override void OnFixedUpdate()
        {
            Blob.CharacterController.UpdateMovement();
        }

        protected override void OnUpdate()
        {
            AdjustLocomotionBlendBasedOnInput();
        }


        private void HandleLieDownInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.LyingDown);
        }

        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput.value == HorizontalInput.Type.Right)
            {
                Blob.CharacterController.HorizontalInput = 1.0f;
            }
            else if (_horizontalInput.value == HorizontalInput.Type.Left)
            {
                Blob.CharacterController.HorizontalInput = -1.0f;
            }
            else
            {
                Blob.CharacterController.HorizontalInput = 0.0f;
            }
        }

        private void AdjustLocomotionBlendBasedOnInput()
        {
            float adjustedBlendAmount = Mathf.Approximately(Blob.CharacterController.HorizontalInput, 0f)?
                Mathf.Clamp01(_locomotionBlend - Blob.OnFeetSettings.locomotionBlendStep) :
                Mathf.Clamp01(_locomotionBlend + Blob.OnFeetSettings.locomotionBlendStep);

            if (!Mathf.Approximately(_locomotionBlend, adjustedBlendAmount))
            {
                _locomotionBlend = adjustedBlendAmount;
                //Blob.Animation.SetFloat(PenguinAnimationParamId.LocomotionIntensity, _locomotionBlend);
            }
        }
    }
}

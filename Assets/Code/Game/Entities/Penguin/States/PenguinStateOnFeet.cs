using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState<PenguinStateId, PenguinBlob>
    {
        public PenguinStateOnFeet() : base() { }

        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.lieDownCommand,                    HandleLieDownInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange,               HandleMoveHorizontalChanged);

            //RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.JumpLiftOff), HandleJumpLiftOff);
            //RegisterEvent(Blob.EventBus.jumpCommand,                       HandleJumpInputReceived);
            //RegisterEvent(Blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged); // disabled until we fix ground handling
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
            Blob.Animation.SetFloat(PenguinAnimationParamId.LocomotionIntensity, _locomotionBlend);
        }


        protected override void OnFixedUpdate()
        {
            // no op
        }

        protected override void OnAnimatorRootMotionUpdate()
        {
            // no op
        }

        protected override void OnAnimatorIkPassUpdate(int layerIndex)
        {
            // no op
        }

        protected override void OnUpdate()
        {
            HandleHorizontalMovement();
        }

        protected override void OnLateUpdate()
        {
            // no op
        }



        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void HandleLieDownInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.LyingDown);
        }

        private void HandleGroundContactChanged(bool isGrounded)
        {
            Blob.Animation.SetBool(PenguinAnimationParamId.IsGrounded, isGrounded);
            if (!isGrounded)
            {
                base.SignalMoveToNextState(PenguinStateId.Midair);
            }
        }


        private void HandleJumpInputReceived()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.JumpUp);
        }

        private void HandleJumpLiftOff()
        {
            Blob.CharacterController.Jump();
        }


        // todo: find a flexible solution for all this duplicated movement code in multiple states
        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput.value == HorizontalInput.Type.Right)
            {
                Blob.CharacterController.FaceRight();
            }
            else if (_horizontalInput.value == HorizontalInput.Type.Left)
            {
                Blob.CharacterController.FaceLeft();
            }
        }

        private void HandleHorizontalMovement()
        {
            if (_horizontalInput.value == HorizontalInput.Type.None)
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend - Blob.OnFeetSettings.LocomotionBlendStep);
            }
            else
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend + Blob.OnFeetSettings.LocomotionBlendStep);
            }

            // todo: abstract locomotion blend as some sort of max speed blend with damping and put in character controller
            // in this case, comparing floats is okay since we assume that values are _only_ adjusted via clamp01
            if (_locomotionBlend != 0.00f)
            {
                // todo: move rigidbody force/movement calls to character controller 2d
                //MoveHorizontal(penguinRigidbody, _xMotionIntensity * _maxInputSpeed, Time.deltaTime);
                Blob.CharacterController.MoveForward();
            }

            Blob.Animation.SetFloat(PenguinAnimationParamId.LocomotionIntensity, _locomotionBlend);
        }
    }
}

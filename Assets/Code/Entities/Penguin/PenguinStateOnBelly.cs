using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinBlob>
    {
        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnBelly(string id, PenguinBlob blob) : base(id, blob) { }
        protected override FsmState<PenguinBlob> OnCreate(string id, PenguinBlob data) => new PenguinStateOnBelly(id, data);


        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.standUpCommand,                    HandleStandUpInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange,               HandleMoveHorizontalChanged);
            RegisterEvent(Blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged);
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
            Blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }

        protected override void OnUpdate()
        {
            HandleHorizontalMovement();
        }


        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void HandleGroundContactChanged(bool isGrounded) => Blob.Animation.SetParamIsGrounded(isGrounded);
        private void HandleStandUpInputReceived() => base.SignalMoveToNextState(PenguinBlob.StateIdStandingUp);

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
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend - Blob.Animation.LocomotionBlendStep);
            }
            else
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend + Blob.Animation.LocomotionBlendStep);
            }

            // todo: abstract locomotion blend as some sort of max speed blend with damping and put in character controller
            // in this case, comparing floats is okay since we assume that values are _only_ adjusted via clamp01
            if (_locomotionBlend != 0.00f)
            {
                // todo: move rigidbody force/movement calls to character controller 2d
                //MoveHorizontal(penguinRigidbody, _xMotionIntensity * _maxInputSpeed, Time.deltaTime);
            }

            Blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }
    }
}

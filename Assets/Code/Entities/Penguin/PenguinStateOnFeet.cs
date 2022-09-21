using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState
    {
        private PenguinFsmDriver _driver;
        private PenguinBlob _blob;

        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnFeet(string name, PenguinFsmDriver driver, PenguinBlob blob)
            : base(name)
        {
            _blob = blob;
            _driver = driver;
        }

        protected override void OnIntialize()
        {
            RegisterEvent(_blob.Animation.JumpLiftOff,                      HandleJumpLiftOff);
            RegisterEvent(_blob.EventBus.jumpCommand,                       HandleJumpInputReceived);
            RegisterEvent(_blob.EventBus.lieDownCommand,                    HandleLieDownInputReceived);
            RegisterEvent(_blob.EventBus.movementInputChange,               HandleMoveHorizontalChanged);
            RegisterEvent(_blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged);
        }

        protected override void OnEnter()
        {
            _blob.CharacterController.Settings = _blob.OnFeetSettings;
            _locomotionBlend = 0.0f;
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        protected override void OnExit()
        {
            _locomotionBlend = 0.0f;
            _horizontalInput = new(HorizontalInput.Type.None);
            _blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }

        protected override void OnUpdate()
        {
            HandleHorizontalMovement();
        }


        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void HandleLieDownInputReceived()
        {
            _driver.MoveToState(PenguinBlob.StateIdLyingDown);
        }

        private void HandleGroundContactChanged(bool isGrounded)
        {
            _blob.Animation.SetParamIsGrounded(isGrounded);
            if (!isGrounded)
            {
                _driver.MoveToState(PenguinBlob.StateIdMidair);
            }
        }


        private void HandleJumpInputReceived()
        {
            _blob.Animation.TriggerParamJumpUpParameter();
        }

        private void HandleJumpLiftOff()
        {
            _blob.CharacterController.Jump();
        }


        // todo: find a flexible solution for all this duplicated movement code in multiple states
        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput.value == HorizontalInput.Type.Right)
            {
                _blob.CharacterController.FaceRight();
            }
            else if (_horizontalInput.value == HorizontalInput.Type.Left)
            {
                _blob.CharacterController.FaceLeft();
            }
        }

        private void HandleHorizontalMovement()
        {
            if (_horizontalInput.value == HorizontalInput.Type.None)
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend - _blob.Animation.LocomotionBlendStep);
            }
            else
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend + _blob.Animation.LocomotionBlendStep);
            }

            // todo: abstract locomotion blend as some sort of max speed blend with damping and put in character controller
            // in this case, comparing floats is okay since we assume that values are _only_ adjusted via clamp01
            if (_locomotionBlend != 0.00f)
            {
                // todo: move rigidbody force/movement calls to character controller 2d
                //MoveHorizontal(penguinRigidbody, _xMotionIntensity * _maxInputSpeed, Time.deltaTime);
            }

            _blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }
    }
}

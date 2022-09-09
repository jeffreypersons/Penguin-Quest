using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnFeet(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        public override void OnEnter()
        {
            _blob.Animation.JumpLiftOff      .AddListener(OnJumpLiftOff);
            _eventCenter.jumpCommand         .AddListener(OnJumpInputReceived);
            _eventCenter.lieDownCommand      .AddListener(OnLieDownInputReceived);
            _eventCenter.movementInputChanged.AddListener(OnMoveHorizontalChanged);
            _blob.CharacterController.GroundContactChanged.AddListener(OnGroundContactChanged);

            _blob.CharacterController.Settings = _blob.OnFeetSettings;
            _locomotionBlend = 0.0f;
            _horizontalInput = HorizontalInput.None;
        }

        public override void OnExit()
        {
            _blob.Animation.JumpLiftOff      .RemoveListener(OnJumpLiftOff);
            _eventCenter.jumpCommand         .RemoveListener(OnJumpInputReceived);
            _eventCenter.lieDownCommand      .RemoveListener(OnLieDownInputReceived);
            _eventCenter.movementInputChanged.RemoveListener(OnMoveHorizontalChanged);
            _blob.CharacterController.GroundContactChanged.RemoveListener(OnGroundContactChanged);

            _locomotionBlend = 0.0f;
            _horizontalInput = HorizontalInput.None;
            _blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }

        public override void OnUpdate()
        {
            HandleHorizontalMovement();
        }

        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void OnLieDownInputReceived(string _)
        {
            _driver.MoveToState(_driver.StateLyingDown);
        }

        private void OnGroundContactChanged(bool isGrounded)
        {
            _blob.Animation.SetParamIsGrounded(isGrounded);
            if (!isGrounded)
            {
                _driver.MoveToState(_driver.StateMidair);
            }
        }


        private void OnJumpInputReceived(string _)
        {
            _blob.Animation.TriggerParamJumpUpParameter();
        }

        private void OnJumpLiftOff(string _)
        {
            _blob.CharacterController.Jump();
        }


        // todo: find a flexible solution for all this duplicated movement code in multiple states
        private void OnMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput == HorizontalInput.Right)
            {
                _blob.CharacterController.FaceRight();
            }
            else if (_horizontalInput == HorizontalInput.Left)
            {
                _blob.CharacterController.FaceLeft();
            }
        }

        private void HandleHorizontalMovement()
        {
            if (_horizontalInput == HorizontalInput.None)
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

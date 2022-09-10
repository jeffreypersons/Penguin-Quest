﻿using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        private float _locomotionBlend;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnBelly(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name, eventRegistry: null)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }


        public override void OnEnter()
        {
            _eventCenter.standUpCommand      .AddListener(HandleStandUpInputReceived);
            _eventCenter.movementInputChanged.AddListener(HandleHorizontalChanged);
            _blob.CharacterController.GroundContactChanged.AddListener(HandleGroundContactChanged);

            _blob.CharacterController.Settings = _blob.OnBellySettings;
            _locomotionBlend = 0.0f;
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        public override void OnExit()
        {
            _eventCenter.standUpCommand      .RemoveListener(HandleStandUpInputReceived);
            _eventCenter.movementInputChanged.RemoveListener(HandleHorizontalChanged);
            _blob.CharacterController.GroundContactChanged.RemoveListener(HandleGroundContactChanged);

            _locomotionBlend = 0.0f;
            _blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }

        public override void OnUpdate()
        {
            HandleHorizontalMovement();
        }


        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void HandleGroundContactChanged(bool isGrounded) => _blob.Animation.SetParamIsGrounded(isGrounded);
        private void HandleStandUpInputReceived(IEventPayload.Empty _) => _driver.MoveToState(_driver.StateStandingUp);

        // todo: find a flexible solution for all this duplicated movement code in multiple states
        private void HandleHorizontalChanged(HorizontalInput state)
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

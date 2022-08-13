﻿using System;
using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        private float _locomotionBlend;
        private bool _isHorizontalInputActive;

        public PenguinStateOnBelly(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }


        public override void Enter()
        {
            _eventCenter.standUpCommand            .AddListener(OnStandUpInputReceived);
            _eventCenter.startHorizontalMoveCommand.AddListener(OnStartHorizontalMoveInput);
            _eventCenter.stopHorizontalMoveCommand .AddListener(OnStopHorizontalMoveInput);
            _blob.CharacterController.GroundContactChanged += OnGroundContactChanged;

            _blob.CharacterController.Settings = _blob.OnBellySettings;
            _locomotionBlend = 0.0f;
            _isHorizontalInputActive = false;
        }

        public override void Exit()
        {
            _eventCenter.standUpCommand            .RemoveListener(OnStandUpInputReceived);
            _eventCenter.startHorizontalMoveCommand.RemoveListener(OnStartHorizontalMoveInput);
            _eventCenter.stopHorizontalMoveCommand .RemoveListener(OnStopHorizontalMoveInput);
            _blob.CharacterController.GroundContactChanged -= OnGroundContactChanged;

            _locomotionBlend = 0.0f;
            _isHorizontalInputActive = false;
            _blob.Animation.SetParamLocomotionIntensity(_locomotionBlend);
        }

        public override void Update()
        {
            HandleHorizontalMovement();
        }

        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void OnGroundContactChanged(bool isGrounded) => _blob.Animation.SetParamIsGrounded(isGrounded);
        private void OnStandUpInputReceived(string _) => _driver.MoveToState(_driver.StateStandingUp);


        // todo: find a flexible solution for all this duplicated movement code in multiple states
        private void OnStartHorizontalMoveInput(int direction)
        {
            _blob.CharacterController.ChangeFacing((CharacterController2D.Facing)(direction));
            _isHorizontalInputActive = true;
        }
        private void OnStopHorizontalMoveInput(string _)
        {
            _isHorizontalInputActive = false;
        }


        private void HandleHorizontalMovement()
        {
            if (_isHorizontalInputActive)
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend + _blob.Animation.LocomotionBlendStep);
            }
            else
            {
                _locomotionBlend = Mathf.Clamp01(_locomotionBlend - _blob.Animation.LocomotionBlendStep);
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
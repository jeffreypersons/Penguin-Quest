﻿using UnityEngine;
using UnityEngine.InputSystem;
using PQ.Game.Input.Generated;


namespace PQ.Game.Input
{
    /*
    Input receiver that maps gameplay control input to the game event system.

    Assumes that an autogenerated c# class is provided that maps to in-editor-configured
    player input/control settings.
    */
    [System.Serializable]
    [AddComponentMenu("GameplayInputReceiver")]
    public class GameplayInputReceiver : MonoBehaviour
    {
        private UnityPlayerControls _generatedPlayerControls;
        private GameEventCenter _eventCenter;

        private HorizontalInput _horizontalInputState;
        private InputAction _moveHorizontal;
        private InputAction _jumpUp;
        private InputAction _standUp;
        private InputAction _lieDown;
        private InputAction _use;
        private InputAction _fire;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            UnityPlayerControls controls = new UnityPlayerControls();

            _generatedPlayerControls = controls;
            _moveHorizontal = controls.Gameplay.MoveHorizontal;
            _jumpUp         = controls.Gameplay.JumpUp;
            _standUp        = controls.Gameplay.StandUp;
            _lieDown        = controls.Gameplay.LieDown;
            _use            = controls.Gameplay.Fire;
            _fire           = controls.Gameplay.Use;

            _horizontalInputState = new(HorizontalInput.Type.None);
        }


        void OnEnable()
        {
            _generatedPlayerControls.Gameplay.Enable();
            _moveHorizontal.started   += OnMoveHorizontalChanged;
            _moveHorizontal.canceled  += OnMoveHorizontalChanged;
            _jumpUp        .performed += OnJumpUp;
            _standUp       .performed += OnStandUp;
            _lieDown       .performed += OnLieDown;
            _use           .performed += OnUse;
            _fire          .performed += OnFire;
        }

        void OnDisable()
        {
            _generatedPlayerControls.Gameplay.Disable();
            _moveHorizontal.started   -= OnMoveHorizontalChanged;
            _moveHorizontal.canceled  -= OnMoveHorizontalChanged;
            _jumpUp        .performed -= OnJumpUp;
            _standUp       .performed -= OnStandUp;
            _lieDown       .performed -= OnLieDown;
            _use           .performed -= OnUse;
            _fire          .performed -= OnFire;
        }

        private void OnMoveHorizontalChanged(InputAction.CallbackContext context)
        {
            HorizontalInput.Type horizontalInputType;
            float rawValue = context.action.ReadValue<float>();
            if (Mathf.Approximately(rawValue, 0))
            {
                horizontalInputType = HorizontalInput.Type.None;
            }
            else if (rawValue < 0)
            {
                horizontalInputType = HorizontalInput.Type.Left;
            }
            else
            {
                horizontalInputType = HorizontalInput.Type.Right;
            }

            if (_horizontalInputState.type != horizontalInputType)
            {
                _horizontalInputState = new(horizontalInputType);
                _eventCenter.movementInputChange.Raise(_horizontalInputState);
            }
        }

        private void OnJumpUp(InputAction.CallbackContext _)  => _eventCenter.jumpCommand.Raise();
        private void OnStandUp(InputAction.CallbackContext _) => _eventCenter.standUpCommand.Raise();
        private void OnLieDown(InputAction.CallbackContext _) => _eventCenter.lieDownCommand.Raise();
        private void OnUse(InputAction.CallbackContext _)     => _eventCenter.useCommand.Raise();
        private void OnFire(InputAction.CallbackContext _)    => _eventCenter.fireCommand.Raise();
    }
}

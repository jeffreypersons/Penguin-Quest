﻿using System;
using UnityEngine;


// todo: use distances, max speed, and transition percent per step for easier inspector usage..
namespace PQ.Entities.Penguin
{
    public class HorizontalMoveHandler : MonoBehaviour
    {
        private enum Facing { Left = -1, Right = 1}
        
        private static readonly Quaternion ROTATION_FACING_RIGHT = Quaternion.Euler(0,   0, 0);
        private static readonly Quaternion ROTATION_FACING_LEFT  = Quaternion.Euler(0, 180, 0);

        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
                 "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)] [SerializeField] private float _locomotionBlendStep = 0.10f;

        //[Header("Physics Settings")]
        //[Range(0.50f, 100.00f)] [SerializeField] private float _maxInputSpeed = 10.0f;

        private PenguinEntity _penguinEntity;
        private bool _isHorizontalInputActive;
        private float _xMotionIntensity;
        private Facing _facing;

        void Awake()
        {
            _penguinEntity = gameObject.GetComponent<PenguinEntity>();

            _isHorizontalInputActive = false;
            _xMotionIntensity = 0.00f;
            _facing = GetFacing(_penguinEntity.Rigidbody);
        }

        void Update()
        {
            HandleHorizontalMovement();
        }


        private void HandleHorizontalMovement()
        {
            if (_isHorizontalInputActive)
            {
                _xMotionIntensity = Mathf.Clamp01(_xMotionIntensity + _locomotionBlendStep);
            }
            else
            {
                _xMotionIntensity = Mathf.Clamp01(_xMotionIntensity - _locomotionBlendStep);
            }

            // in this case, comparing floats is okay since we assume that values are _only_ adjusted via clamp01
            if (_xMotionIntensity != 0.00f)
            {
                // todo: move rigidbody force/movement calls to character controller 2d
                //MoveHorizontal(penguinRigidbody, _xMotionIntensity * _maxInputSpeed, Time.deltaTime);
            }

            _penguinEntity.Animation.SetParamXMotionIntensity(_xMotionIntensity);
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.startHorizontalMoveCommand.AddListener(OnStartHorizontalMoveInput);
            GameEventCenter.stopHorizontalMoveCommand .AddListener(OnStopHorizontalMoveInput);
        }
        void OnDisable()
        {
            GameEventCenter.startHorizontalMoveCommand.RemoveListener(OnStartHorizontalMoveInput);
            GameEventCenter.stopHorizontalMoveCommand .RemoveListener(OnStopHorizontalMoveInput);
        }

        void OnStartHorizontalMoveInput(int direction)
        {
            TurnToFace((Facing)(direction));
            _isHorizontalInputActive = true;
        }

        void OnStopHorizontalMoveInput(string _)
        {
            _isHorizontalInputActive = false;
        }


        private void TurnToFace(Facing facing)
        {
            // todo: move rigidbody force/movement calls to character controller 2d
            if (this._facing == facing)
            {
                return;
            }

            this._facing = facing;
            switch (this._facing)
            {
                case Facing.Left:
                    transform.localRotation = ROTATION_FACING_LEFT;
                    break;
                case Facing.Right:
                    transform.localRotation = ROTATION_FACING_RIGHT;
                    break;
                default:
                    Debug.LogError($"Given value `{facing}` is not a valid Facing");
                    break;
            }
        }


        private static Facing GetFacing(Rigidbody2D rigidbody)
        {
            return Mathf.Abs(rigidbody.transform.localEulerAngles.y) <= 90.0f ?
                Facing.Right :
                Facing.Left;
        }

        /* Move along forward axis at a given horizontal speed contribution and time delta. */
        private static void MoveHorizontal(Rigidbody2D rigidbody, float speed, float time)
        {
            // todo: might want to do the projected horizontal speed contribution calculations in the
            //       ground handler script rather than here, and just use the same value regardless (same for midair)
            Vector2 movementAxis    = GetFacing(rigidbody) == Facing.Right? Vector2.right : Vector2.left;
            Vector2 currentPosition = rigidbody.transform.position;
            Vector2 currentForward  = rigidbody.transform.forward.normalized;

            // project the given velocity along the forward axis (1 if forward is completely horizontal, 0 if vertical)
            Vector2 displacement = Vector2.Dot(movementAxis, currentForward) * (speed * time) * movementAxis;
            rigidbody.MovePosition(currentPosition + displacement);
        }
    }
}

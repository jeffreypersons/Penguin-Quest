using System;
using UnityEngine;
using PenguinQuest.Data;


// todo: use distances, max speed, and transition percent per step for easier inspector usage..
namespace PenguinQuest.Controllers
{
    public class HorizontalMoveHandler : MonoBehaviour
    {
        private enum Facing { Left = -1, Right = 1}
        
        private static readonly Quaternion ROTATION_FACING_RIGHT = Quaternion.Euler(0,   0, 0);
        private static readonly Quaternion ROTATION_FACING_LEFT  = Quaternion.Euler(0, 180, 0);

        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
                 "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)] [SerializeField] private float locomotionBlendStep = 0.10f;

        //[Header("Physics Settings")]
        //[Range(0.50f, 100.00f)] [SerializeField] private float maxInputSpeed = 10.0f;


        private PenguinEntity penguinEntity;

        private bool   isHorizontalInputActive;
        private float  xMotionIntensity;
        private Facing facing;
        
        void Awake()
        {
            penguinEntity = gameObject.GetComponent<PenguinEntity>();

            xMotionIntensity = 0.00f;
            facing           = GetFacing(penguinEntity.Rigidbody);
        }
        
        void Update()
        {
            HandleHorizontalMovement();
        }


        private void HandleHorizontalMovement()
        {
            if (isHorizontalInputActive)
            {
                xMotionIntensity = Mathf.Clamp01(xMotionIntensity + locomotionBlendStep);
            }
            else
            {
                xMotionIntensity = Mathf.Clamp01(xMotionIntensity - locomotionBlendStep);
            }

            // in this case, comparing floats is okay since we assume that values are _only_ adjusted via clamp01
            if (xMotionIntensity != 0.00f)
            {
                // todo: move rigidbody force/movement calls to character controller 2d
                //MoveHorizontal(penguinRigidbody, xMotionIntensity * maxInputSpeed, Time.deltaTime);
            }

            penguinEntity.Animation.SetParamXMotionIntensity(xMotionIntensity);
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
            isHorizontalInputActive = true;
        }

        void OnStopHorizontalMoveInput(string _)
        {
            isHorizontalInputActive = false;
        }


        private void TurnToFace(Facing facing)
        {
            // todo: move rigidbody force/movement calls to character controller 2d
            if (this.facing == facing)
            {
                return;
            }

            this.facing = facing;
            switch (this.facing)
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

using System;
using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class HorizontalMoveHandler : MonoBehaviour
    {
        private enum Facing { LEFT = -1, RIGHT = 1}

        private const float LOCOMOTION_BLEND_STEP_DEFAULT = 0.10f;
        private const float LOCOMOTION_BLEND_STEP_MIN     = 0.01f;
        private const float LOCOMOTION_BLEND_STEP_MAX     = 1.00f;

        private static readonly Quaternion ROTATION_FACING_RIGHT = Quaternion.Euler(0,   0, 0);
        private static readonly Quaternion ROTATION_FACING_LEFT  = Quaternion.Euler(0, 180, 0);

        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
                 "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(LOCOMOTION_BLEND_STEP_MIN, LOCOMOTION_BLEND_STEP_MAX)]
        [SerializeField] private float locomotionBlendStep = LOCOMOTION_BLEND_STEP_DEFAULT;

        private Rigidbody2D penguinRigidBody;
        private Animator    penguinAnimator;

        private bool  isHorizontalInputActive;
        private float xMotionIntensity;
        private Facing facing;

        void Awake()
        {
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
            xMotionIntensity = 0.00f;
            if (Mathf.Abs(transform.localEulerAngles.y) <= 90.0f)
            {
                facing = Facing.RIGHT;
            }
        }

        void Update()
        {
            if (isHorizontalInputActive)
            {
                xMotionIntensity = Mathf.Clamp01(xMotionIntensity + locomotionBlendStep);
            }
            else
            {
                xMotionIntensity = Mathf.Clamp01(xMotionIntensity - locomotionBlendStep);
            }

            penguinAnimator.SetFloat("XMotionIntensity", xMotionIntensity);
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
            //penguinRigidBody.constraints = RigidbodyConstraints2D.None;
            TurnToFace((Facing)(direction));
            isHorizontalInputActive = true;
        }
        void OnStopHorizontalMoveInput(string _)
        {
            isHorizontalInputActive = false;
        }

        private void TurnToFace(Facing facing)
        {
            if (this.facing == facing)
            {
                return;
            }

            this.facing = facing;
            switch (this.facing)
            {
                case Facing.LEFT:
                    transform.localRotation = ROTATION_FACING_LEFT;
                    break;
                case Facing.RIGHT:
                    transform.localRotation = ROTATION_FACING_RIGHT;
                    break;
                default:
                    Debug.LogError($"Given value `{facing}` is not a valid Facing");
                    break;
            }
        }
    }
}

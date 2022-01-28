using System;
using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpUpHandler : MonoBehaviour
    {
        private const float JUMP_STRENGTH_DEFAULT = 50000.00f;
        private const float JUMP_STRENGTH_MIN     = 25000.00f;
        private const float JUMP_STRENGTH_MAX     = 250000.00f;
        private const float JUMP_ANGLE_DEFAULT    = 45.00f;
        private const float JUMP_ANGLE_MIN        = 0.00f;
        private const float JUMP_ANGLE_MAX        = 90.00f;

        [Header("Jump Settings")]
        [Tooltip("Strength of jump force in newtons")]
        [Range(JUMP_STRENGTH_MIN, JUMP_STRENGTH_MAX)]
        [SerializeField] private float jumpStrength = JUMP_STRENGTH_DEFAULT;

        [Tooltip("Angle to jump (in degrees counterclockwise to the penguin's forward facing direction)")]
        [Range(JUMP_ANGLE_MIN, JUMP_ANGLE_MAX)] [SerializeField] private float jumpAngle = JUMP_ANGLE_DEFAULT;

        private Animator penguinAnimator;
        private Rigidbody2D penguinRigidBody;
        private PenguinAnimationEventReciever animationComponent;

        private Vector2 netImpulseForce;

        void Awake()
        {
            penguinAnimator    = gameObject.GetComponent<Animator>();
            animationComponent = gameObject.GetComponentInChildren<PenguinAnimationEventReciever>();
            penguinRigidBody   = gameObject.GetComponent<Rigidbody2D>();
            netImpulseForce    = Vector2.zero;
        }

        void LateUpdate()
        {
            if (netImpulseForce != Vector2.zero)
            {
                penguinRigidBody.constraints = RigidbodyConstraints2D.None;
                penguinRigidBody.AddForce(netImpulseForce, ForceMode2D.Impulse);
                netImpulseForce = Vector2.zero;
            }
        }


        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.jumpCommand.AddListener(OnJumpInput);
            animationComponent.OnJumpImpulse += OnJumpUpAnimationEventImpulse;
        }

        void OnDisable()
        {
            GameEventCenter.jumpCommand.RemoveListener(OnJumpInput);
        }

        void OnJumpInput(string _)
        {
            penguinAnimator.SetTrigger("JumpUp");
        }

        void OnJumpUpAnimationEventImpulse()
        {
            // clear jump trigger to avoid triggering a jump after landing,
            // in the case that jump is pressed twice in a row
            penguinAnimator.ResetTrigger("JumpUp");
            float angleFromGround = jumpAngle * Mathf.Deg2Rad;
            netImpulseForce += jumpStrength * new Vector2(Mathf.Cos(angleFromGround), Mathf.Sin(angleFromGround));
        }
    }
}

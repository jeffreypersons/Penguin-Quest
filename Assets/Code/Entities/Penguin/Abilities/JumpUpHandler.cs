using System;
using UnityEngine;


namespace PQ.Entities.Penguin
{
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
        [SerializeField] private float _jumpStrength = JUMP_STRENGTH_DEFAULT;

        [Tooltip("Angle to jump (in degrees counterclockwise to the penguin's forward facing direction)")]
        [Range(JUMP_ANGLE_MIN, JUMP_ANGLE_MAX)] [SerializeField] private float _jumpAngle = JUMP_ANGLE_DEFAULT;


        private Vector2 _netImpulseForce;
        private PenguinBlob _penguinEntity;

        void Awake()
        {            
            _penguinEntity   = transform.GetComponent<PenguinBlob>();
            _netImpulseForce = Vector2.zero;
        }

        void LateUpdate()
        {
            if (_netImpulseForce != Vector2.zero)
            {
                _penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.None;
                _penguinEntity.Rigidbody.AddForce(_netImpulseForce, ForceMode2D.Impulse);
                _netImpulseForce = Vector2.zero;
            }
        }


        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.jumpCommand.AddListener(OnJumpInputReceived);
            _penguinEntity.Animation.JumpLiftOff += ApplyJumpImpulse;
        }

        void OnDisable()
        {
            GameEventCenter.jumpCommand.RemoveListener(OnJumpInputReceived);
            _penguinEntity.Animation.JumpLiftOff -= ApplyJumpImpulse;
        }

        void OnJumpInputReceived(string _)
        {
            _penguinEntity.Animation.TriggerParamJumpUpParameter();
        }

        void ApplyJumpImpulse()
        {
            // todo: move rigidbody force/movement calls to character controller 2d
            // clear jump trigger to avoid triggering a jump after landing,
            // in the case that jump is pressed twice in a row
            _penguinEntity.Animation.ResetAllTriggers();
            float angleFromGround = _jumpAngle * Mathf.Deg2Rad;
            _netImpulseForce += _jumpStrength * new Vector2(Mathf.Cos(angleFromGround), Mathf.Sin(angleFromGround));
        }
    }
}

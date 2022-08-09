using UnityEngine;


namespace PQ.Entities.Penguin
{
    public class JumpUpHandler : MonoBehaviour
    {
        private Vector2 _netImpulseForce;
        private PenguinBlob _penguinBlob;

        private GameEventCenter _eventCenter;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            _penguinBlob = transform.GetComponent<PenguinBlob>();
            _netImpulseForce = Vector2.zero;
        }

        void LateUpdate()
        {
            if (_netImpulseForce != Vector2.zero)
            {
                _penguinBlob.Rigidbody.constraints = RigidbodyConstraints2D.None;
                _penguinBlob.Rigidbody.AddForce(_netImpulseForce, ForceMode2D.Impulse);
                _netImpulseForce = Vector2.zero;
            }
        }


        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            _eventCenter.jumpCommand.AddListener(OnJumpInputReceived);
            _penguinBlob.Animation.JumpLiftOff += ApplyJumpImpulse;
        }

        void OnDisable()
        {
            _eventCenter.jumpCommand.RemoveListener(OnJumpInputReceived);
            _penguinBlob.Animation.JumpLiftOff -= ApplyJumpImpulse;
        }

        void OnJumpInputReceived(string _)
        {
            _penguinBlob.Animation.TriggerParamJumpUpParameter();
        }

        void ApplyJumpImpulse()
        {
            // clear jump trigger to avoid triggering a jump after landing,
            // in the case that jump is pressed twice in a row
            _penguinBlob.Animation.ResetAllTriggers();

            // todo: move rigidbody force/movement calls to character controller 2d
            float angle     = _penguinBlob.CharacterController.Settings.JumpAngle * Mathf.Deg2Rad;
            float magnitude = _penguinBlob.CharacterController.Settings.JumpStrength;
            _netImpulseForce += magnitude * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}

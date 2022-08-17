using UnityEngine;
using PQ.Common;
using PQ.Common.Collisions;


namespace PQ.Entities.Placeholder
{
    public class RectEntity : MonoBehaviour
    {
        private RectBlob _blob;
        private GameEventCenter _eventCenter;
        private RectMovement _movement;

        private bool _isHorizontalInputActive;

        [SerializeField] private RayCasterSettings _castSettings;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;
            _isHorizontalInputActive = false;
            _movement = new RectMovement(transform, _castSettings);
        }

        void OnEnable()
        {
            _eventCenter.startHorizontalMoveCommand.AddListener(OnMoveHorizontalStarted);
            _eventCenter.stopHorizontalMoveCommand .AddListener(OnMoveHorizontalStopped);
            _eventCenter.jumpCommand               .AddListener(OnJump);
        }

        void OnDisable()
        {
            _eventCenter.startHorizontalMoveCommand.RemoveListener(OnMoveHorizontalStarted);
            _eventCenter.stopHorizontalMoveCommand .RemoveListener(OnMoveHorizontalStopped);
            _eventCenter.jumpCommand               .RemoveListener(OnJump);
        }

        void FixedUpdate()
        {
            if (_isHorizontalInputActive)
            {
                _movement.MoveForwardForTime(Time.fixedDeltaTime);
            }
        }

        private void OnMoveHorizontalStarted(int direction)
        {
            _isHorizontalInputActive = true;
            if (direction == 1)
            {
                _movement.FaceRight();
            }
            else
            {
                _movement.FaceLeft();
            }

        }
        private void OnMoveHorizontalStopped(string _)
        {
            _isHorizontalInputActive = false;
        }

        private void OnJump(string _)
        {

        }
    }
}

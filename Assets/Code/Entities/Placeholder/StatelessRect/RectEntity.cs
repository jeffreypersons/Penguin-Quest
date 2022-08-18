using UnityEngine;
using PQ.Common;
using PQ.Common.Physics;
using PQ.Common.Collisions;


namespace PQ.Entities.Placeholder
{
    public class RectEntity : MonoBehaviour
    {
        private RectBlob _blob;
        private GameEventCenter _eventCenter;
        private Movement _movement;

        private HorizontalInput _horizontalInput;

        void Awake()
        {
            _blob            = gameObject.GetComponent<RectBlob>();
            _eventCenter     = GameEventCenter.Instance;
            _movement        = new Movement(_blob.Transform, _blob.CastSettings);
            _horizontalInput = HorizontalInput.None;
        }

        void OnEnable()
        {
            _eventCenter.movementInputChanged.AddListener(OnMoveHorizontalChanged);
            _eventCenter.jumpCommand         .AddListener(OnJump);
        }

        void OnDisable()
        {
            _eventCenter.movementInputChanged.RemoveListener(OnMoveHorizontalChanged);
            _eventCenter.jumpCommand         .RemoveListener(OnJump);
        }

        void Update()
        {
            if (_horizontalInput != HorizontalInput.None)
            {
                _movement.MoveForwardForTime(Time.deltaTime);
            }
        }

        private void OnMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput == HorizontalInput.Right)
            {
                _movement.FaceRight();
            }
            else if (_horizontalInput == HorizontalInput.Left)
            {
                _movement.FaceLeft();
            }
        }

        private void OnJump(string _)
        {

        }
    }
}

using UnityEngine;
using PQ.Common;
using PQ.Entities;


namespace PQ.TestScenes.Minimal
{
    public class RectEntity : MonoBehaviour
    {
        [SerializeField] private Character2DSettings _characterSettings;

        private GameEventCenter _eventCenter;
        private RectMovementController _controller;

        private HorizontalInput _horizontalInput;

        void Awake()
        {
            _controller = gameObject.GetComponent<RectMovementController>();
            _eventCenter = GameEventCenter.Instance;
            _horizontalInput = new(HorizontalInput.Type.None);

            _controller.Settings = _characterSettings;
        }

        void OnEnable()
        {
            _eventCenter.jumpCommand.AddHandler(OnJump);
            _eventCenter.movementInputChanged.AddHandler(OnMoveHorizontalChanged);
        }

        void OnDisable()
        {
            _eventCenter.jumpCommand.RemoveHandler(OnJump);
            _eventCenter.movementInputChanged.RemoveHandler(OnMoveHorizontalChanged);
        }

        void Update()
        {
            if (_horizontalInput.value != HorizontalInput.Type.None)
            {
                _controller.MoveForward();
            }
        }

        private void OnMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput.value == HorizontalInput.Type.Right)
            {
                _controller.FaceRight();
            }
            else if (_horizontalInput.value == HorizontalInput.Type.Left)
            {
                _controller.FaceLeft();
            }
        }

        private void OnJump() { }
    }
}

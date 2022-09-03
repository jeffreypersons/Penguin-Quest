using UnityEngine;
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
            _horizontalInput = HorizontalInput.None;

            _controller.Settings = _characterSettings;
        }

        void OnEnable()
        {
            _eventCenter.jumpCommand.AddListener(OnJump);
            _eventCenter.movementInputChanged.AddListener(OnMoveHorizontalChanged);
        }

        void OnDisable()
        {
            _eventCenter.jumpCommand.RemoveListener(OnJump);
            _eventCenter.movementInputChanged.RemoveListener(OnMoveHorizontalChanged);
        }

        void Update()
        {
            if (_horizontalInput != HorizontalInput.None)
            {
                _controller.MoveForward();
            }
        }

        private void OnMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput == HorizontalInput.Right)
            {
                _controller.FaceRight();
            }
            else if (_horizontalInput == HorizontalInput.Left)
            {
                _controller.FaceLeft();
            }
        }

        private void OnJump(string _) { }
    }
}

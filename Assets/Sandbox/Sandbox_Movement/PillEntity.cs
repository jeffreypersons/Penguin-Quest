using UnityEngine;
using PQ.Game;
using PQ.Game.Entities;


namespace PQ.TestScenes.Movement
{
    public class PillEntity : MonoBehaviour
    {
        [SerializeField] private CharacterEntitySettings _characterSettings;

        private GameEventCenter _eventCenter;
        private PillMovement _controller;

        private HorizontalInput _horizontalInput;

        void Awake()
        {
            _controller = gameObject.GetComponent<PillMovement>();
            _eventCenter = GameEventCenter.Instance;
            _horizontalInput = new(HorizontalInput.Type.None);

            _controller.Settings = _characterSettings;
        }

        void OnEnable()
        {
            _eventCenter.jumpCommand.AddHandler(OnJump);
            _eventCenter.movementInputChange.AddHandler(OnMoveHorizontalChanged);
        }

        void OnDisable()
        {
            _eventCenter.jumpCommand.RemoveHandler(OnJump);
            _eventCenter.movementInputChange.RemoveHandler(OnMoveHorizontalChanged);
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

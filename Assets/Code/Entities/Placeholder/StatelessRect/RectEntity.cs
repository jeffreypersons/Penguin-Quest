using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Placeholder
{
    public class RectEntity : MonoBehaviour
    {
        private RectBlob _blob;
        private GameEventCenter _eventCenter;
        private HorizontalInput _horizontalInput;

        void Awake()
        {
            _blob            = gameObject.GetComponent<RectBlob>();
            _eventCenter     = GameEventCenter.Instance;
            _horizontalInput = HorizontalInput.None;
            _blob.CharacterController.Settings = _blob.CharacterSettings;
        }
        void Start()
        {
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
                _blob.CharacterController.MoveForward();
            }
        }

        private void OnMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
            if (_horizontalInput == HorizontalInput.Right)
            {
                _blob.CharacterController.FaceRight();
            }
            else if (_horizontalInput == HorizontalInput.Left)
            {
                _blob.CharacterController.FaceLeft();
            }
        }

        private void OnJump(string _) { }
    }
}

using System;
using System.ComponentModel;
using UnityEngine;
using PQ.Common.Collisions;
using Command = PQ.Common.Command<PQ.Common.EmptyArgs>;


namespace PQ.Common
{
    public class CharacterController2D : MonoBehaviour
    {
        private enum Facing { Left, Right };

        private GameObject2D _gameObject2D;
        private CollisionChecker2D _collisionChecker;
        
        public override string ToString() =>
            $"CharacterController2D@{_gameObject2D}";

        bool _isCurrentlyContactingGround;
        private Command<Facing> _turnCommand;
        private Command         _moveCommand;

        void Awake()
        {
            _gameObject2D = new GameObject2D(transform);
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();

            _isCurrentlyContactingGround = false;

            _turnCommand = new(ExecuteTurn);
            _moveCommand = new(ExecuteMove);
        }

        public event Action<bool> GroundContactChanged;
        public CharacterController2DSettings Settings { get; set; }

        public void FaceLeft()    => _turnCommand.Request(Facing.Left);
        public void FaceRight()   => _turnCommand.Request(Facing.Right);
        public void MoveForward() => _moveCommand.Request();

        private void Start()
        {
            if (Settings == null)
            {
                throw new InvalidOperationException("Character controller settings not set");
            }
            UpdateGroundContactInfo();
        }

        void Update()
        {
            Debug.Log(this);
            UpdateGroundContactInfo();

            _turnCommand.ExecuteIfRequested();
            _moveCommand.ExecuteIfRequested();
        }

        private void UpdateGroundContactInfo(bool force = false)
        {
            if (_isCurrentlyContactingGround != _collisionChecker.IsGrounded || force)
            {
                _isCurrentlyContactingGround = _collisionChecker.IsGrounded;
                GroundContactChanged?.Invoke(_isCurrentlyContactingGround);
            }
        }

        private void ExecuteTurn(Facing facing)
        {
            float degreesAboutYAxis = facing switch
            {
                Facing.Right =>   0,
                Facing.Left  => 180,
                _ => throw new InvalidEnumArgumentException(),
            };
            _gameObject2D.SetOrientation3D(0, degreesAboutYAxis, 0);
        }

        private void ExecuteMove()
        {
            _gameObject2D.MoveForward(Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime);
        }
    }
}

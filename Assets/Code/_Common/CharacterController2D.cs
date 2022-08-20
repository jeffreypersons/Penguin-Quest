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

        private KinematicBody2D _kinematicBody2D;
        private CollisionChecker2D _collisionChecker;
        
        public override string ToString() =>
            $"CharacterController2D@{_kinematicBody2D}";

        bool _isCurrentlyContactingGround;
        private Command<Facing> _turnCommand;
        private Command         _moveCommand;

        void Awake()
        {
            _kinematicBody2D  = gameObject.GetComponent<KinematicBody2D>();
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();

            _isCurrentlyContactingGround = false;

            _turnCommand = new(ExecuteFacingChange);
            _moveCommand = new(ExecuteHorizontalMove);
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

        private void ExecuteFacingChange(Facing facing)
        {
            float degreesAboutYAxis = facing switch
            {
                Facing.Right =>   0,
                Facing.Left  => 180,
                _ => throw new InvalidEnumArgumentException(),
            };
            _kinematicBody2D.SetLocalOrientation3D(0, degreesAboutYAxis, 0);
        }

        private void ExecuteHorizontalMove()
        {
            _kinematicBody2D.MoveForward(Settings.HorizontalMovementPeakSpeed * Time.smoothDeltaTime);
        }
    }
}

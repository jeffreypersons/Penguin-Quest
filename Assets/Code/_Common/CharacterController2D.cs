using System;
using UnityEngine;
using PQ.Common.Collisions;


namespace PQ.Common
{
    public class CharacterController2D : MonoBehaviour
    {
        private KinematicBody2D _kinematicBody2D;
        private CollisionChecker2D _collisionChecker;
        
        public override string ToString() =>
            $"CharacterController2D@{_kinematicBody2D}";

        bool _isCurrentlyContactingGround;

        void Awake()
        {
            _kinematicBody2D  = gameObject.GetComponent<KinematicBody2D>();
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();

            _isCurrentlyContactingGround = false;
        }

        public event Action<bool> GroundContactChanged;
        public CharacterController2DSettings Settings { get; set; }

        public void FaceRight() => _kinematicBody2D.SetLocalOrientation3D(0, 0, 0);
        public void FaceLeft() => _kinematicBody2D.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward() =>
            _kinematicBody2D.MoveForward(Settings.HorizontalMovementPeakSpeed * Time.smoothDeltaTime);

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
            UpdateGroundContactInfo();
        }

        private void UpdateGroundContactInfo(bool force = false)
        {
            if (_isCurrentlyContactingGround != _collisionChecker.IsGrounded || force)
            {
                _isCurrentlyContactingGround = _collisionChecker.IsGrounded;
                GroundContactChanged?.Invoke(_isCurrentlyContactingGround);
            }
        }
    }
}

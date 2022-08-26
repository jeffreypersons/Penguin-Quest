using System;
using UnityEngine;
using PQ.Common.Collisions;


namespace PQ.Common
{
    public class KinematicCharacter2D : MonoBehaviour
    {
        private CollisionChecker2D _collisionChecker;

        private bool _isGrounded;
        private KinematicBody2D _kinematicBody2D;

        public event Action<bool> GroundContactChanged;
        public KinematicCharacter2DSettings Settings { get; set; }

        public void FaceRight() => _kinematicBody2D.SetLocalOrientation3D(0, 0, 0);
        public void FaceLeft()  => _kinematicBody2D.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward()
        {
            float distanceToMove = Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime;
            _kinematicBody2D.MoveBy(distanceToMove * _kinematicBody2D.Forward);
        }


        void Awake()
        {
            _kinematicBody2D  = gameObject.GetComponent<KinematicBody2D>();
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();
        }

        void Start()
        {
            if (Settings == null)
            {
                throw new InvalidOperationException("Character controller settings not set");
            }
            UpdateGroundContactInfo();
        }

        void FixedUpdate()
        {
            UpdateGroundContactInfo();

            if (!_isGrounded)
            {
                _kinematicBody2D.MoveBy(Settings.GravityStrength * Vector2.down);
            }
        }


        private void UpdateGroundContactInfo(bool force = false)
        {
            bool isInContactWithGround = _collisionChecker.IsGrounded;
            if (_isGrounded != isInContactWithGround || force)
            {
                _isGrounded = isInContactWithGround;
                GroundContactChanged?.Invoke(_isGrounded);
            }
        }
    }
}

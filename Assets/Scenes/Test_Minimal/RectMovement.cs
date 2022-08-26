using System;
using UnityEngine;
using PQ.Common;
using PQ.Common.Collisions;


namespace PQ.TestScenes.Minimal
{
    public class RectMovement : MonoBehaviour
    {
        private CollisionChecker2D _collisionChecker;

        private bool _isGrounded;
        private KinematicBody2D _kinematicBody2D;

        public event Action<bool> GroundContactChanged;
        public KinematicCharacter2DSettings Settings { get; set; }

        public Vector2 Position => _kinematicBody2D.Position;
        public void PlaceAt(Vector2 position, float rotation)
        {
            _kinematicBody2D.MoveTo(position);
            _kinematicBody2D.SetLocalOrientation3D(0, 0, rotation);
        }
        public void FaceRight() => _kinematicBody2D.SetLocalOrientation3D(0, 0, 0);
        public void FaceLeft() => _kinematicBody2D.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward()
        {
            float distanceToMove = Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime;
            _kinematicBody2D.MoveBy(distanceToMove * _kinematicBody2D.Forward);
        }
        public void Jump()
        {
            // todo: replace with actual jump code
        }

        void Awake()
        {
            _kinematicBody2D = gameObject.GetComponent<KinematicBody2D>();
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();
        }

        void Start()
        {
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

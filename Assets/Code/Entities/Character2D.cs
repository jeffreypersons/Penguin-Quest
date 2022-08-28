using System;
using UnityEngine;
using PQ.Common;
using PQ.Common.Collisions;


namespace PQ.Entities
{
    public class Character2D : MonoBehaviour
    {
        private CollisionChecker2D _collisionChecker;

        private bool _isGrounded;
        private PhysicsBody2D _body;

        public event Action<bool> GroundContactChanged;
        public Character2DSettings Settings { get; set; }

        public void PlaceAt(Vector2 position, float rotation)
        {
            _body.MoveTo(position);
            _body.SetLocalOrientation3D(0, 0, rotation);
        }
        public void FaceRight() => _body.SetLocalOrientation3D(0, 0, 0);
        public void FaceLeft()  => _body.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward()
        {
            float distanceToMove = Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime;
            _body.MoveBy(distanceToMove * _body.Forward);
        }
        public void Jump()
        {
            // todo: replace with actual jump code
        }

        void Awake()
        {
            _body = gameObject.GetComponent<PhysicsBody2D>();
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
                _body.MoveBy(Settings.GravityStrength * Vector2.down);
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

using System;
using UnityEngine;
using PQ.Common.Collisions;
using PQ.Common.Physics;


namespace PQ.Common
{
    // todo: add more character movement settings for things like speed and jump height
    public class CharacterController2D_v2 : MonoBehaviour
    {
        private BoxCollider2D      _boundingBox;
        private Rigidbody2D        _rigidbody;
        private Transform2D        _movement;
        private CollisionChecker2D _collisionChecker;

        private bool _isCurrentlyContactingGround;
        private bool _moveRequested;

        public CharacterController2DSettings Settings { get; set; }
        public event Action<bool> GroundContactChanged;

        public void FaceLeft()    => _movement.SetOrientation3D(0, 0, 180);
        public void FaceRight()   => _movement.SetOrientation3D(0, 0, 0);
        public void MoveForward() => _moveRequested = true;

        void Awake()
        {
            _boundingBox      = gameObject.GetComponent<BoxCollider2D>();
            _rigidbody        = gameObject.GetComponent<Rigidbody2D>();
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();
            _movement         = new Transform2D(transform);
        }

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

        void FixedUpdate()
        {
            if (_moveRequested)
            {
                _movement.MoveForward(Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime);
                _moveRequested = false;
            }
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

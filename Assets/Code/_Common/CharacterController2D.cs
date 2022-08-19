using System;
using UnityEngine;
using PQ.Common.Collisions;


namespace PQ.Common
{
    public class CharacterController2D : MonoBehaviour
    {
        private GameObject2D       _gameObject2D;
        private CollisionChecker2D _collisionChecker;

        private bool _isCurrentlyContactingGround;
        private bool _moveRequested;

        public CharacterController2DSettings Settings { get; set; }
        public event Action<bool> GroundContactChanged;

        public void FaceLeft()    => _gameObject2D.SetOrientation3D(0, 0, 180);
        public void FaceRight()   => _gameObject2D.SetOrientation3D(0, 0, 0);
        public void MoveForward() => _moveRequested = true;

        void Awake()
        {
            _gameObject2D = new GameObject2D(transform);
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();
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
                _gameObject2D.MoveForward(Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime);
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

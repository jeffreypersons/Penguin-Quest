using System;
using UnityEngine;


namespace PQ.Common
{
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField] private ContactFilter2D _groundContactFilter;

        private bool _isGrounded;
        private KinematicBody2D _kinematicBody2D;

        void Awake()
        {
            _kinematicBody2D = gameObject.GetComponent<KinematicBody2D>();
        }

        public event Action<bool> GroundContactChanged;
        public CharacterController2DSettings Settings { get; set; }

        public void FaceRight() => _kinematicBody2D.SetLocalOrientation3D(0, 0, 0);
        public void FaceLeft()  => _kinematicBody2D.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward()
        {
            float distanceToMove = Settings.HorizontalMovementPeakSpeed * Time.smoothDeltaTime;
            _kinematicBody2D.MoveBy(distanceToMove * _kinematicBody2D.Forward);
        }

        private void Start()
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
            bool isInContactWithGround = _kinematicBody2D.IsTouching(_groundContactFilter);
            if (_isGrounded != isInContactWithGround || force)
            {
                _isGrounded = isInContactWithGround;
                GroundContactChanged?.Invoke(_isGrounded);
            }
        }
    }
}

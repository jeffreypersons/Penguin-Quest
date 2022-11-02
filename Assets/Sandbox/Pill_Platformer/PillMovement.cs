using UnityEngine;
using PQ.Common.Physics;
using PQ.Common.Events;
using PQ.Game.Entities;


namespace PQ.TestScenes.Movement
{
    public class PillMovement : MonoBehaviour
    {
        private KinematicMover2D _mover;

        private bool _isGrounded;
        private PqEvent<bool> _groundContactChangedEvent = new("character2D.groundContact.changed");

        public CharacterEntitySettings Settings { get; set; }
        public IPqEventReceiver<bool> OnGroundContactChanged => _groundContactChangedEvent;

        public void PlaceAt(Vector2 position, float rotation) => _mover.PlaceAt(position, rotation);
        public void FaceRight()   => _mover.Flip(horizontal: false, vertical: false);
        public void FaceLeft()    => _mover.Flip(horizontal: true,  vertical: false);
        public void MoveForward() => _mover.MoveBy(Settings.HorizontalMovementPeakSpeed * _mover.Extrapolated.Forward);
        public void Jump()        => _mover.MoveBy(Settings.JumpDistanceToPeak * _mover.Extrapolated.Above);

        void Awake()
        {
            _mover = gameObject.GetComponent<KinematicMover2D>();
            if (_mover == null)
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }
        }

        void Start()
        {
            _mover.CastOffset = -5f;
            UpdateGroundContactInfo(force: true);
        }

        void Update()
        {
            UpdateGroundContactInfo();
            if (!_isGrounded)
            {
                //_mover.MoveBy(Settings.GravityStrength * Vector2.down);
            }
        }


        private void UpdateGroundContactInfo(bool force = false)
        {
            // todo: use a scriptable object or something for these variables
            var result = _mover.CastBelow(
                xOffset:  0f,
                mask:     LayerMask.GetMask("Platform"),
                distance: 5);

            bool isInContactWithGround = result.HitInRange(0f, Settings.MaxToleratedDistanceFromGround);
            if (_isGrounded != isInContactWithGround || force)
            {
                _isGrounded = isInContactWithGround;
                _groundContactChangedEvent.Raise(_isGrounded);
            }
        }
    }
}

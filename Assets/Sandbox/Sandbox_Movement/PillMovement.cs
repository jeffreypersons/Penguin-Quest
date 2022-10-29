using UnityEngine;
using PQ.Common.Casts;
using PQ.Common.Physics;
using PQ.Common.Events;
using PQ.Game.Entities;


namespace PQ.TestScenes.Movement
{
    public class PillMovement : MonoBehaviour
    {
        private RayCasterBox _caster;

        private float _amountToMoveForward;
        private bool _isGrounded;
        private KinematicBody2D _body;
        private PqEvent<bool> _groundContactChangedEvent = new("character2D.groundContact.changed");

        public CharacterEntitySettings Settings { get; set; }
        public IPqEventReceiver<bool> OnGroundContactChanged => _groundContactChangedEvent;

        public void PlaceAt(Vector2 position, float rotation)
        {
            _body.MoveTo(position);
            _body.SetLocalOrientation3D(0, 0, rotation);
        }
        public void FaceRight() => _body.SetLocalOrientation3D(0,   0, 0);
        public void FaceLeft()  => _body.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward()
        {
            _amountToMoveForward = Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime;
        }
        public void Jump()
        {
            // todo: replace with actual jump code
        }

        void Awake()
        {
            _body = gameObject.GetComponent<KinematicBody2D>();
            _caster = new RayCasterBox(_body);
            _caster.CastOffset   = -5f;
            _amountToMoveForward =  0f;
        }

        void Start()
        {
            UpdateGroundContactInfo(force: true);
        }

        void FixedUpdate()
        {
            UpdateGroundContactInfo();

            if (!_isGrounded)
            {
                Debug.Log($"{Settings.GravityStrength * Vector2.down}");
                _body.MoveBy(Settings.GravityStrength * Vector2.down);
            }
            _body.MoveBy(_amountToMoveForward * _body.Forward);
            _amountToMoveForward = 0f;
        }


        private void UpdateGroundContactInfo(bool force = false)
        {
            // todo: use a scriptable object or something for these variables
            var result = _caster.CastBelow(
                t:        0.50f,
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

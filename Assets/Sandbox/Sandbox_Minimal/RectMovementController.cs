using UnityEngine;
using PQ.Common.Casts;
using PQ.Common.Physics;
using PQ.Game.Entities;


namespace PQ.TestScenes.Minimal
{
    public class RectMovementController : MonoBehaviour
    {
        private RayCasterBox _caster;
        private KinematicBody2D _body;

        private bool _isGrounded;

        public Vector2 Position => _body.Position;
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
            _body   = gameObject.GetComponent<KinematicBody2D>();
            _caster = new RayCasterBox(_body);
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
                _body.MoveBy(Settings.GravityStrength * Vector2.down);
            }
        }


        private void UpdateGroundContactInfo(bool force = false)
        {
            // todo: use a scriptable object or something for these checks
            var result = _caster.CheckBelow(t: 0.50f, mask: LayerMask.GetMask("Platform"), 10f);
            bool isInContactWithGround = result.distance >= 0.50f;
            if (_isGrounded != isInContactWithGround || force)
            {
                _isGrounded = isInContactWithGround;
            }
        }
    }
}

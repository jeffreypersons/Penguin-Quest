using System;
using UnityEngine;
using PQ.Common;
using PQ.Common.Extensions;


namespace PQ.Entities
{
    public class Character2D : MonoBehaviour
    {
        private RayCasterBox _caster;

        private bool _isGrounded;
        private KinematicBody2D _body;

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
            _body = gameObject.GetComponent<KinematicBody2D>();
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
            // collider is turned off - check back later
            if (_body.BoundExtents == Vector2.zero)
            {
                return;
            }

            // todo: use a scriptable object or something for these checks
            var result = _caster.CheckBelow(target: LayerMask.GetMask("Platform"), distance: int.MaxValue);
            bool isInContactWithGround = result.hitPercentage >= 0.50f && result.hitDistance <= 0.25f;

            if (_isGrounded != isInContactWithGround || force)
            {
                _isGrounded = isInContactWithGround;
                GroundContactChanged?.Invoke(_isGrounded);
            }
        }
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window
            GizmoExtensions.DrawLine(_caster.BackSide,   Color.gray);
            GizmoExtensions.DrawLine(_caster.FrontSide,  Color.gray);
            GizmoExtensions.DrawLine(_caster.BottomSide, Color.gray);
            GizmoExtensions.DrawLine(_caster.TopSide,    Color.gray);

            // draw a pair of arrows from the that should be identical to the transform's axes in the editor window
            GizmoExtensions.DrawArrow(from: _caster.Center, to: _caster.Center + _caster.ForwardAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: _caster.Center, to: _caster.Center + _caster.UpAxis,      color: Color.green);
        }
        #endif
    }
}

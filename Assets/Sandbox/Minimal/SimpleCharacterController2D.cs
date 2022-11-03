using System;
using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class SimpleCharacterController2D : ICharacterController2D
    {
        private bool _flipped;
        private bool _isGrounded;
        private Vector2 _position;
        private Vector2 _forward;
        private Vector2 _up;

        private const int HitBufferSize = 16;

        private readonly ContactFilter2D   _contactFilter;
        private readonly RaycastHit2D[]    _hitBuffer;
        private readonly Rigidbody2D       _rigidBody;
        private readonly CapsuleCollider2D _capsule;

        public SimpleCharacterController2D(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException($"Expected non-null game object");
            }
            if (!gameObject.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {gameObject}");
            }
            if (!gameObject.TryGetComponent<CapsuleCollider2D>(out var capsule))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }

            _contactFilter = new();
            _hitBuffer     = new RaycastHit2D[HitBufferSize];
            _rigidBody     = rigidBody;
            _capsule       = capsule;

            _rigidBody.isKinematic = true;
            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            _contactFilter.useLayerMask = true;

            _flipped = false;
            SetXYOrientation(degreesAroundXAxis: 0f, degreesAroundYAxis: 0f);
            SyncPropertiesFromRigidBody();
        }

        Vector2 ICharacterController2D.Position   => _position;
        Vector2 ICharacterController2D.Forward    => _forward;
        Vector2 ICharacterController2D.Up         => _up;
        bool    ICharacterController2D.IsGrounded => _isGrounded;
        bool    ICharacterController2D.Flipped    => _flipped;

        void ICharacterController2D.Flip()
        {
            _flipped = !_flipped;
            SetXYOrientation(degreesAroundXAxis: 0f, degreesAroundYAxis: _flipped ? -180f : 0f);
            SyncPropertiesFromRigidBody();
        }

        void ICharacterController2D.Move(Vector2 deltaPosition)
        {
            _rigidBody.position += new Vector2(deltaPosition.x, deltaPosition.y);
            SyncPropertiesFromRigidBody();
        }

        
        private void SetXYOrientation(float degreesAroundXAxis, float degreesAroundYAxis)
        {
            _rigidBody.transform.localEulerAngles =
                new Vector3(degreesAroundXAxis, degreesAroundYAxis, _rigidBody.transform.localEulerAngles.z);
        }
        
        private void SyncPropertiesFromRigidBody()
        {
            _position = _rigidBody.transform.position;
            _forward  = _rigidBody.transform.right.normalized;
            _up       = _rigidBody.transform.up.normalized;
        }
    }
}

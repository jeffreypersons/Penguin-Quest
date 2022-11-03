using System;
using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public sealed class SimpleCharacterController2D : ICharacterController2D
    {
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
        }

        bool ICharacterController2D.IsGrounded { get; }

        void ICharacterController2D.Move(float deltaX, float deltaY)
        {
            if (Mathf.Approximately(deltaX, 0f) && Mathf.Approximately(deltaY, 0f))
            {
                return;
            }

            _rigidBody.position += new Vector2(deltaX, deltaY);
        }
    }
}

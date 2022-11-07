using System;
using UnityEngine;
using PQ.TestScenes.Minimal.Physics;


namespace PQ.TestScenes.Minimal
{
    public class SimpleCharacterController2D : ICharacterController2D
    {
        private bool _flipped;
        private bool _isGrounded;
        private bool _slideOnGround;
        private bool _slideOnCeilings;
        private LinearPhysicsSolver2D _solver;

        public SimpleCharacterController2D(GameObject gameObject, ContactFilter2D contactFilter,
            float contactOffset, int maxIterations, float maxSlopeAngle)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException($"Expected non-null game object");
            }
            if (!gameObject.TryGetComponent<Rigidbody2D>(out var body))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {gameObject}");
            }
            if (!gameObject.TryGetComponent<BoxCollider2D>(out var box))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }

            _flipped         = false;
            _isGrounded      = false;
            _slideOnGround   = false;
            _slideOnCeilings = false;
            _solver = new(body, box, contactFilter, contactOffset, maxIterations, maxSlopeAngle, _slideOnGround, _slideOnCeilings);
        }

        Vector2 ICharacterController2D.Position      => _solver.Body.position;
        Bounds  ICharacterController2D.Bounds        => _solver.AAB;
        Vector2 ICharacterController2D.Forward       => _solver.Body.transform.right.normalized;
        Vector2 ICharacterController2D.Up            => _solver.Body.transform.up.normalized;
        bool    ICharacterController2D.IsGrounded    => _isGrounded;
        bool    ICharacterController2D.Flipped       => _flipped;
        float   ICharacterController2D.ContactOffset => _solver.ContactOffset;

        public static bool DrawCastsInEditor              { get; set; } = true;
        public static bool DrawMovementResolutionInEditor { get; set; } = true;

        void ICharacterController2D.Flip()
        {
            _flipped = !_flipped;
            _solver.Flip(horizontal: _flipped, vertical: false);
        }

        void ICharacterController2D.Move(Vector2 deltaPosition)
        {
            _solver.Move(deltaPosition);
        }
    }
}

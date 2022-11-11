using UnityEngine;
using PQ.TestScenes.Minimal.Physics;


namespace PQ.TestScenes.Minimal
{
    public class SimpleCharacterController2D : ICharacterController2D
    {
        private bool _flipped;
        private bool _isGrounded;
        private readonly KinematicBody2D _body;
        private readonly LinearPhysicsSolver2D _solver;

        public SimpleCharacterController2D(GameObject gameObject, in SolverParams solverParams)
        {
            if (!gameObject.TryGetComponent<KinematicBody2D>(out var body))
            {
                throw new MissingComponentException($"Expected non-null {nameof(KinematicBody2D)}");
            }

            _flipped    = false;
            _isGrounded = false;
            _body       = body;
            _solver     = new LinearPhysicsSolver2D(body, solverParams);

            body.SetSkinWidth(solverParams.ContactOffset);
        }

        Vector2 ICharacterController2D.Position   => _body.Position;
        Vector2 ICharacterController2D.Forward    => _body.Right;
        Vector2 ICharacterController2D.Up         => _body.Up;
        bool    ICharacterController2D.IsGrounded => _isGrounded;
        bool    ICharacterController2D.Flipped    => _flipped;

        void ICharacterController2D.Flip()
        {
            _body.SetSkinWidth(_solver.Params.ContactOffset);
            _flipped = !_flipped;
            _body.Flip(horizontal: _flipped, vertical: false);
        }

        void ICharacterController2D.Move(Vector2 deltaPosition)
        {
            _body.SetSkinWidth(_solver.Params.ContactOffset);
            _solver.Move(deltaPosition);
        }
    }
}

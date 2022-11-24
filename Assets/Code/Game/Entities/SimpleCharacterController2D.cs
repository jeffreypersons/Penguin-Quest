using UnityEngine;
using PQ.Common.Physics;


namespace PQ.Game.Entities
{
    public sealed class SimpleCharacterController2D : ICharacterController2D
    {
        private readonly KinematicBody2D _body;
        private readonly CollideAndSlideSolver2D _solver;

        public SimpleCharacterController2D(GameObject gameObject, in SolverParams solverParams)
        {
            if (!gameObject.TryGetComponent<KinematicBody2D>(out var body))
            {
                throw new MissingComponentException($"Expected non-null {nameof(KinematicBody2D)}");
            }

            _body   = body;
            _solver = new CollideAndSlideSolver2D(body, solverParams);

            _body.SetSkinWidth(_solver.Params.ContactOffset);
        }

        Vector2 ICharacterController2D.Position   => _body.Position;
        Vector2 ICharacterController2D.Forward    => _body.Forward;
        Vector2 ICharacterController2D.Up         => _body.Up;
        bool    ICharacterController2D.IsGrounded => (_solver.Flags & CollisionFlags2D.Below) != 0;
        bool    ICharacterController2D.Flipped    => _body.FlippedHorizontal;

        void ICharacterController2D.Flip()
        {
            _body.SetSkinWidth(_solver.Params.ContactOffset);
            _body.Flip(horizontal: !_body.FlippedHorizontal, vertical: false);
        }

        void ICharacterController2D.Move(Vector2 deltaPosition)
        {
            _body.SetSkinWidth(_solver.Params.ContactOffset);
            _solver.Move(deltaPosition);
        }
    }
}

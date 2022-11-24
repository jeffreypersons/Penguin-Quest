using UnityEngine;


namespace PQ.Game.Entities
{
    public interface ICharacterController2D
    {
        public Vector2 Position      { get; }
        public Vector2 Forward       { get; }
        public Vector2 Up            { get; }
        public bool    IsGrounded    { get; }
        public bool    Flipped       { get; }

        public void Flip();
        public void Move(Vector2 deltaPosition);
    }
}

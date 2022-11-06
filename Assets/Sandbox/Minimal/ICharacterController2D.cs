using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public interface ICharacterController2D
    {
        public Vector2 Position      { get; }
        public Bounds  Bounds        { get; }
        public Vector2 Forward       { get; }
        public Vector2 Up            { get; }
        public bool    IsGrounded    { get; }
        public bool    Flipped       { get; }
        public float   ContactOffset { get; }

        public void Flip();
        public void Move(Vector2 deltaPosition);
    }
}

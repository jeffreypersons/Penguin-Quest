

namespace PQ.TestScenes.Minimal
{
    public interface ICharacterController2D
    {
        public bool IsGrounded { get; }

        public void Move(float deltaX, float deltaY);
    }
}

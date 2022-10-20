

namespace PQ.Game.Entities.Penguin
{
    // currently, each id must correspond _exactly_ - order/name/count -
    // to animator parameter (in future we will sync dynamically)
    public enum PenguinAnimationParamId
    {
        LocomotionIntensity,
        SlopeIntensity,
        IsGrounded,
        LieDown,
        StandUp,
        JumpUp,
        Fire,
        Use,
    }
}

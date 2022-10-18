

namespace PQ.Game.Entities.Penguin
{
    public enum PenguinAnimationParamId
    {
        Locomotion,
        SlopeIntensity,
        IsGrounded,
        TriggerLieDown,
        TriggerStandUp,
        TriggerJumpUp,
        TriggerFire,
        TriggerUse,
    }


    /*
    Reminder: These parameters _must_ match the names in mecanim.
    Unfortunately there is no easy way to generate the parameter names,
    so just be careful to make sure that they match the parameters listed in the Unity Animator.
    */
    // todo: look into validation of the param names with the animator using below enums
    public static class PenguinAnimationParamNames
    {
        public static readonly string paramLocomotion = "LocomotionIntensity";
        public static readonly string paramSlope      = "SlopeIntensity";
        public static readonly string paramIsGrounded = "IsGrounded";
        public static readonly string paramLie        = "LieDown";
        public static readonly string paramStand      = "StandUp";
        public static readonly string paramJump       = "JumpUp";
        public static readonly string paramFire       = "Fire";
        public static readonly string paramUse        = "Use";
    }
}

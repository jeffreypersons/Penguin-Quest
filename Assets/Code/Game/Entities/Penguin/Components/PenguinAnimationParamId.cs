

namespace PQ.Game.Entities.Penguin
{
    /*
    Reminder: These parameters _must_ match the names in mecanim.
    Unfortunately there is no easy way to generate the parameter names,
    so just be careful to make sure that they match the parameters listed in the Unity Animator.
    */
    // todo: look into validation of the param names with the animator using below enums
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
}

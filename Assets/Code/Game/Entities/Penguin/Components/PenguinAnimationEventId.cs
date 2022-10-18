

namespace PQ.Game.Entities.Penguin
{
    // rather than hold a bunch of properties, we use these IDs for looking up which events to trigger (from animator),
    // or listen to (from client code), greatly consolidating animation boilerplate
    // 
    public enum PenguinAnimationEventId
    {
        JumpLiftOff,
        LieDownStarted,
        LieDownMidpoint,
        LieDownEnded,
        StandUpStarted,
        StandUpEnded,
        Fired,
        Used,
        FrontFootDown,
    }

    /*
    Reminder: These parameters _must_ match the names in mecanim.
    Unfortunately there is no easy way to generate the parameter names,
    so just be careful to make sure that they match the parameters listed in the Unity Animator.
    */
    // todo: look into validation of the param names with the animator using below enums
    public enum Params
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

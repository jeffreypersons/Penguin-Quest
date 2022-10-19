using PQ.Common.Containers;


namespace PQ.Game.Entities.Penguin
{
    public enum PenguinAnimationParamId
    {
        LocomotionIntensity,
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

    todo: try adding params like done in this doc example: https://docs.unity3d.com/ScriptReference/Animations.AnimatorController.html
          eg controller.AddParameter("Fire", AnimatorControllerParameterType.Trigger);

          doing this will entail some custom editor scripting, possible locking on animator values, and so forth.
          
    */
    // todo: look into validation of the param names with the animator using above enums
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

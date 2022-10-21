

namespace PQ.Game.Entities.Penguin
{
    // rather than hold a bunch of properties, we use these IDs for looking up which events to trigger (from animator),
    // or listen to (from client code), greatly consolidating animation boilerplate
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
        WalkingFrontFootUp,
        WalkingFrontFootDown,
        WalkingFrontAndBackFootAligned,
        WalkingBackFootUp,
        WalkingBackFootDown,
        BellySlideFlippersAndBellyAligned,
        BellySlideFlippersExtended,
    }
}

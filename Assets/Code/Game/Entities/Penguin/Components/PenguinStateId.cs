

namespace PQ.Game.Entities.Penguin
{
    // note - until (and if) we add custom serializer hooks to get these stored via their string values, instead of ints,
    //        then preferably we should add to the end to avoid all references from being renamed
    //        ..but if it's not exposed in the editor, it's fine.
    public enum PenguinStateId
    {
        Feet,
        Belly,
        StandingUp,
        LyingDown,
        Midair,
    }
}

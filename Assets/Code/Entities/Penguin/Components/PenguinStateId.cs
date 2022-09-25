using System;


namespace PQ.Entities.Penguin
{
    [Flags]
    public enum PenguinStateId
    {
        Feet       = 0,
        Belly      = 1 << 1,
        StandingUp = 1 << 2,
        LyingDown  = 1 << 3,
        Midair     = 1 << 4,
    }
}

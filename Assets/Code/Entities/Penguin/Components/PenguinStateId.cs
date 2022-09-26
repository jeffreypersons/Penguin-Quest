using System;


namespace PQ.Entities.Penguin
{
    [Flags]
    public enum PenguinStateId
    {
        None       = 0,
        Feet       = 1 << 1,
        Belly      = 1 << 2,
        StandingUp = 1 << 3,
        LyingDown  = 1 << 4,
        Midair     = 1 << 5,
    }
}

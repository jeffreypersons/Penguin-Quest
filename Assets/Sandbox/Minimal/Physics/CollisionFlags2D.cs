using System;


namespace PQ.TestScenes.Minimal.Physics
{
    [Flags]
    public enum CollisionFlags2D
    {
        None       = 0,
        Front      = 1 << 1,
        Bottom     = 1 << 2,
        Back       = 1 << 3,
        Top        = 1 << 4,
        SteepPoly  = 1 << 5,
        SlightPoly = 1 << 6,
        All        = ~0,
    }
}

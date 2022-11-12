using System;


namespace PQ.TestScenes.Minimal.Physics
{
    [Flags]
    public enum CollisionFlags2D
    {
        None   = 0,
        Front  = 1 << 1,
        Below  = 1 << 2,
        Behind = 1 << 3,
        Above  = 1 << 4,
        All    = ~0,
    }
}

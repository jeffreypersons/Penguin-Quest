

namespace PQ.Entities.Penguin
{
    [System.Flags]
    public enum PenguinColliderConstraints
    {
        None            = 0,
        DisableHead     = 1 << 1,
        DisableTorso    = 1 << 2,
        DisableFlippers = 1 << 3,
        DisableFeet     = 1 << 4,
        DisableOuter    = 1 << 5,
        DisableAll      = ~0,
    }
}

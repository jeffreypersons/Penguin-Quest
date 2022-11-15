

namespace PQ.Game.Entities.Penguin
{
    /*
    // todo: replace the flags enum with the below, and then use an enum set for flags.
    //       note that this will be less trivial of a replacement than the other enums were.
    //
    //       This is because we will need to write a custom editor drawer for enumsets that have equivalent
    //       functionality of the enum flag selection in Unity inspector, with None and All options
    //       and multi-select. But once we have that in place, we can replace the flags here with a less error prone:
    public enum PenguinColliderConstraints
    {
        DisableHead,
        DisableTorso,
        DisableFlippers,
        DisableFeet,
        DisableOuter,
    }
    */
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

using UnityEngine;
using PenguinQuest.Extensions;


namespace PenguinQuest.Controllers
{
    [System.Flags]
    public enum PenguinColliderConstraints
    {
        None               = 0,
        DisableHead        = 1 << 1,
        DisableTorso       = 1 << 2,
        DisableFlippers    = 1 << 3,
        DisableFeet        = 1 << 4,
        DisableBoundingBox = 1 << 5,
        DisableAll         = ~0,
    }
}

using UnityEngine;


namespace PQ.Game.Peformance
{
    [CreateAssetMenu(
        fileName = "RuntimeSettings",
        menuName = "ScriptableObjects/RuntimeSettings",
        order    = 1)]
    public class RuntimeSettings : ScriptableObject
    {
        public int TargetFrameRate => _usePlatformDefaultFrameRate? -1 : _customTargetFrameRate;

        [Header("Performance Settings")]
        [Tooltip("How many frames per second should we aim for as a default?")]
        [Range(30, 120)][SerializeField] private int _customTargetFrameRate = 60;

        [Tooltip("Should we use the above target framerate, or just use platform default (eg 30 fps for Android)?")]
        [SerializeField] private bool _usePlatformDefaultFrameRate = false;
    }
}

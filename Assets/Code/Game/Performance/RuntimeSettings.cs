using UnityEngine;


namespace PQ.Game.Peformance
{
    [CreateAssetMenu(
        fileName = "RuntimeSettings",
        menuName = "ScriptableObjects/RuntimeSettings",
        order    = 1)]
    public class RuntimeSettings : ScriptableObject
    {
        public int TargetFrameRate    => _usePlatformDefaultFrameRate? -1 : _customTargetFrameRate;



        [Header("Frame Rates")]

        [Tooltip("Should we use the above target framerate, or just use platform default (eg 30 fps for Android)?")]
        [SerializeField] private bool _usePlatformDefaultFrameRate = false;

        [Tooltip("If not default, then how many frames per second should we aim for?")]
        [Range(30, 120)][SerializeField] private int _customTargetFrameRate = 60;
    }
}

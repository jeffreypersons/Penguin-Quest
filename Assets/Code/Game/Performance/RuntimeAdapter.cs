using UnityEngine;


namespace PQ.Game.Peformance
{
    // todo: extend this to provide platform specific overrides
    /*
    Runtime adapter for performance and synchronizing other game-wide and/or platform specific settings.
    */
    public class RuntimeAdapter : MonoBehaviour
    {
        [SerializeField] private RuntimeSettings _settings;

        void Awake()
        {
            UpdateCurrentSettings();
            Debug.Log(
                $"Starting up {GetType()} with quality level {QualitySettings.GetQualityLevel()} and " +
                $"target frame-rate {Application.targetFrameRate}");
        }

        void OnValidate()
        {
            UpdateCurrentSettings();
        }

        private void UpdateCurrentSettings()
        {
            //
            // For all current conceivable cases, we never want to await vertical synchronization to
            // occur between frames, as it can effectively cap frame rate by doing so by matching platform refresh.
            // 
            // Instead, we allow overriding of specific (or all) platform's default target application frame rate.
            // Note that in spite of these precautions, some platforms such as IOS still force v-sync, and there
            // is no actual way to turn it off.
            //
            //
            if (QualitySettings.vSyncCount != 0)
            {
                QualitySettings.vSyncCount = 0;
                Debug.Log($"Disabled v-sync passes between frames");
            }

            int target = _settings.TargetFrameRate;
            if (Application.targetFrameRate != target)
            {
                Application.targetFrameRate = target;
                Debug.LogFormat($"Set target frame-rate to {0}",
                    target == -1 ? "platform default" : target);
            }
        }
    }
}

using System;
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

        private string QualityInfo =>
            $"quality level {QualitySettings.GetQualityLevel()} of" +
            $"[{string.Join(", ", QualitySettings.names)}]";

        private int VSyncCount      { get => QualitySettings.vSyncCount;  set => QualitySettings.vSyncCount = value;  }
        private int TargetFrameRate { get => Application.targetFrameRate; set => Application.targetFrameRate = value; }


        void Awake()
        {
            UpdateCurrentSettings();
            Debug.Log(
                $"Starting up {GetType()} with target frame-rate {TargetFrameRate} and {QualityInfo}");
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
            if (VSyncCount != 0)
            {
                VSyncCount = 0;
                Debug.Log($"Disabled v-sync passes between frames");
            }

            int targetFps = _settings.TargetFrameRate;
            if (TargetFrameRate != targetFps)
            {
                TargetFrameRate = targetFps;
                Debug.LogFormat($"Set target frame-rate to {0}",
                    targetFps == -1 ? "platform default" : targetFps);
            }
        }
    }
}

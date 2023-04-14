using System;
using UnityEngine;


namespace PQ.Game.Peformance
{
    // todo: look into possibly extending this to provide platform specific overrides
    /*
    Runtime adapter for performance and synchronizing other game-wide and/or platform specific settings.
    */
    public class RuntimeAdapter : MonoBehaviour
    {
        [SerializeField] private RuntimeSettings _settings;


        private int VSyncCount      { get => QualitySettings.vSyncCount;  set => QualitySettings.vSyncCount  = value; }
        private int TargetFrameRate { get => Application.targetFrameRate; set => Application.targetFrameRate = value; }
        private string QualityInfo =>
            $"quality level {QualitySettings.GetQualityLevel()} of " +
            $"[{string.Join(", ", QualitySettings.names)}]";


        private void Awake()
        {
            // todo: add frame timing stats and all that
            //Debug.Log($"OnAwake : Gathering GC info.." + ReportGarbageCollection());
            UpdateCurrentSettings();
            Debug.Log($"Starting up {GetType()} with target frame-rate {TargetFrameRate} and {QualityInfo}");
        }

        void Start()
        {
            // todo: replace below disabled gc log with more useful reporting (maybe using profiler api?)
            //Debug.Log($"OnStart : Gathering GC info.." + ReportGarbageCollection());
        }

        private void OnDestroy()
        {
            // todo: replace below disabled gc log with more useful reporting (maybe using profiler api?)
            //Debug.Log($"OnDestroy : Gathering GC info.." + ReportGarbageCollection());
        }

        void OnValidate()
        {
            UpdateCurrentSettings();
        }

        private void UpdateCurrentSettings()
        {
            // There are some cases where it starts up in editor after a reimport where the application's target
            // framerate is temporarily zeroed out. Since we only care about the true starting target frame rate when the
            // game actually starts, we can skip updates for those cases.
            if (!Application.IsPlaying(this) || TargetFrameRate == 0)
            {
                return;
            }

            
            // For all current conceivable cases, we never want to await vertical synchronization to
            // occur between frames, as it can effectively cap frame rate by doing so by matching platform refresh.
            // 
            // Instead, we allow overriding of specific (or all) platform's default target application frame rate.
            // Note that in spite of these precautions, some platforms such as IOS still force v-sync, and there
            // is no actual way to turn it off.
            if (VSyncCount != 0)
            {
                VSyncCount = 0;
                Debug.Log($"Non-zero vsync count detected - disabled v-sync passes between frames");
            }

            int targetFps = _settings.TargetFrameRate;
            if (TargetFrameRate != targetFps)
            {
                TargetFrameRate = targetFps;
                Debug.LogFormat($"Set target frame-rate to {0}",
                    targetFps == -1 ? "platform default" : targetFps);
            }
        }

        // todo: replace below disabled gc reporting (maybe using profiler api?)
        private string ReportGarbageCollection()
        {
            var gcCount1stGen = GC.CollectionCount(generation: 0);
            var gcTotalMemory = GC.GetTotalMemory(forceFullCollection: false);
            return $"\n****** Current Garbage Collection Stats ******\n" +
                   $"GC Count[gen-0] : {gcCount1stGen}\n" +
                   $"Approximate GC memory : {gcTotalMemory}\n";
        }
    }
}

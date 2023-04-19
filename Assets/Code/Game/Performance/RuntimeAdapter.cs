using UnityEngine;


namespace PQ.Game.Peformance
{
    // todo: look into possibly extending this to provide platform specific overrides
    // todo: add frame timing stats and [useful!] gc stats
    /*
    Runtime adapter for performance and synchronizing other game-wide and/or platform specific settings.


    Note that not all settings are configurable - some like vsync count we never use so don't expose a
    shortcut setting for it - but others like TimeScale that's useful for debugging is exposed directly.
    */
    public class RuntimeAdapter : MonoBehaviour
    {
        [SerializeField] private RuntimeSettings _settings;

        private int   VSyncCount      { get => QualitySettings.vSyncCount;        set => QualitySettings.vSyncCount  = value;    }
        private int   TargetFrameRate { get => Application.targetFrameRate;       set => Application.targetFrameRate = value;    }
        private int   QualityLevel    { get => QualitySettings.GetQualityLevel(); set => QualitySettings.SetQualityLevel(value); }
        private float TimeScale       { get => Time.timeScale;                    set => Time.timeScale = value;                 }

        public override string ToString()
        {
            string[] qualities = QualitySettings.names;
            qualities[QualityLevel] = $"{qualities[QualityLevel]}[x]";

            string vSync     = VSyncCount > 0 ? "on" : "off";
            string fps       = TargetFrameRate == -1 ? "platform default" : $"{TargetFrameRate}";
            string quality   = $"[{string.Join(',', qualities)}]";
            string timeScale = $"{TimeScale}";
            return $"{GetType()}(VSync:{vSync}, TargetFps:{fps}, Quality:{quality}, TimeScale:{timeScale})";
        }

        void Awake()
        {
            _settings.OnChanged = UpdateCurrentSettings;

            UpdateCurrentSettings();
            Debug.Log($"Starting up {this}");
        }

        private void UpdateCurrentSettings()
        {
            // For all current conceivable cases, we never want to await vertical synchronization to
            // occur between frames, as it can effectively cap frame rate by doing so by matching platform refresh rate.
            // 
            // Instead, we allow overriding of specific (or all) platform's default target application frame rate.
            // Note that in spite of these precautions, some platforms such as IOS still force v-sync, and there
            // is no actual way to turn it off.
            if (VSyncCount != 0)
            {
                VSyncCount = 0;
                Debug.Log($"Non-zero vsync count detected - disabled v-sync passes between frames");
            }

            int targetFps = _settings.usePlatformDefaultFrameRate? -1 : _settings.customTargetFrameRate;
            if (TargetFrameRate != targetFps)
            {
                TargetFrameRate = targetFps;
            }

            TimeScale = _settings.timeScale;
        }
    }
}

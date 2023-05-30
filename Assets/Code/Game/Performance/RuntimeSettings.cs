using System;
using UnityEngine;
using PQ.Common.Tuning;


namespace PQ.Game.Peformance
{
    [CreateAssetMenu(fileName="RuntimeSettings", menuName="TuningConfigs/RuntimeSettings", order=1)]
    public sealed class RuntimeSettings : TuningConfig
    {
        // todo: add a custom editor attribute used for tuning config instances, to denote non-persistent fields
        [Header("Timing")]
        [Tooltip("Time scale used during runtime (useful for debugging movement)")]
        [Range(0.10f, 5f)][SerializeField] public float timeScale = 1.00f;


        [Header("Frame Rates")]

        [Tooltip("Should we use the above target framerate, or just use platform default (eg 30 fps for Android)?")]
        [SerializeField] public bool usePlatformDefaultFrameRate = false;

        [Tooltip("If not default, then how many frames per second should we aim for?")]
        [Range(30, 120)][SerializeField] public int customTargetFrameRate = 60;
    }
}

using System;
using UnityEngine;


namespace PQ.Game.Peformance
{
    [CreateAssetMenu(
        fileName = "RuntimeSettings",
        menuName = "ScriptableObjects/RuntimeSettings",
        order    = 1)]
    public class RuntimeSettings : ScriptableObject
    {
        // Any time script is loaded, or field values are modified, where do we want that input to go?
        // Note that since we treat there editor-configured settings as input, we deliberately expose a single listener at a time
        public Action OnChanged { set; get; }
        private void OnValidate() => OnChanged?.Invoke();


        [Header("Timing")]

        [Tooltip("What time scale to use (note useful to scale down when debugging movement)?")]
        [Range(0.10f, 5f)][SerializeField] public float timeScale = 1.00f;


        [Header("Frame Rates")]

        [Tooltip("Should we use the above target framerate, or just use platform default (eg 30 fps for Android)?")]
        [SerializeField] public bool usePlatformDefaultFrameRate = false;

        [Tooltip("If not default, then how many frames per second should we aim for?")]
        [Range(30, 120)][SerializeField] public int customTargetFrameRate = 60;
    }
}

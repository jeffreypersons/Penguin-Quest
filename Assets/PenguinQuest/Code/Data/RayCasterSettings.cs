using UnityEngine;


namespace PenguinQuest.Data
{
    [CreateAssetMenu(
        fileName = "CharacterControllerSettings",
        menuName = "ScriptableObjects/RayCasterSettings",
        order    = 1)]
    public class RayCasterSettings : ScriptableObject
    {
        [Header("Cast Settings")]

        [Tooltip("How much distance between each ray (if relevant)?")]
        [SerializeField] [Range(0.00f, 100.00f)] private float  raySpacing  = 0.25f;

        [Tooltip("What delta along the ray do we start the cast from?")]
        [SerializeField] [Range(-10.00f, 10.00f)] private float rayOffset   = 0.25f;

        [Tooltip("At what max distance do we send a cast until?")]
        [SerializeField] [Range(0.00f, 1000.00f)] private float maxDistance = 2.50f;

        [Tooltip("What layers do we take into consider when testing for a ray hit?")]
        [SerializeField] private LayerMask targetLayers = ~0;

        public float     Offset       => rayOffset;
        public float     MaxDistance  => maxDistance;
        public LayerMask TargetLayers => targetLayers;
        public float     RaySpacing   => raySpacing;
    }
}

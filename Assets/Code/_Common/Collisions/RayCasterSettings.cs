using UnityEngine;


namespace PQ.Common.Collisions
{
    [CreateAssetMenu(
        fileName = "RayCasterSettings",
        menuName = "ScriptableObjects/RayCasterSettings",
        order    = 1)]
    public class RayCasterSettings : ScriptableObject
    {
        public float     Offset              => rayCastOffset;
        public float     MaxDistance         => maxRayDistance;
        public LayerMask TargetLayers        => targetLayers;
        public float     DistanceBetweenRays => distanceBetweenRays;



        [Header("Cast Settings")]

        [Tooltip("How much distance between each ray (if relevant)?")]
        [SerializeField] [Range(0.10f, 100.00f)] private float  distanceBetweenRays = 0.25f;

        [Tooltip("What delta along the ray do we start the cast from?")]
        [SerializeField] [Range(-10.00f, 10.00f)] private float rayCastOffset       = 0.25f;

        [Tooltip("At what max distance do we send a cast until?")]
        [SerializeField] [Range(0.00f, 1000.00f)] private float maxRayDistance      = 2.50f;

        [Tooltip("What layers do we take into consider when testing for a ray hit?")]
        [SerializeField] private LayerMask targetLayers = ~0;
    }
}

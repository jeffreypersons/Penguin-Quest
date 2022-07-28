using UnityEngine;


namespace PenguinQuest.Framework
{
    [CreateAssetMenu(
        fileName = "CharacterControllerSettings",
        menuName = "ScriptableObjects/CharacterController2DSettings",
        order    = 1)]
    public class CharacterController2DSettings : ScriptableObject
    {
        public float MaxAscendableSlopeAngle                      => maxAscendableSlopeAngle;
        public float MaxToleratedDistanceFromGround               => maxToleratedDistanceFromGround;

        public bool  MaintainPerpendicularityToSurface            => maintainPerpendicularityToSurface;
        public float SurfaceAlignmentRotationalStrength           => surfaceAlignmentRotationalStrength;
        public float DegreesFromSurfaceNormalThreshold            => degreesFromSurfaceNormalThreshold;

        public bool  EnableAutomaticAxisLockingForSmallVelocities => enableAutomaticAxisLockingForSmallVelocities;
        public float LinearVelocityThreshold                      => linearVelocityThreshold;
        public float AngularVelocityThreshold                     => angularVelocityThreshold;
        


        [Header("Surface Handling Ranges")]

        [Tooltip("At what slope angle do we allow the penguin to walk up to?")]
        [SerializeField] [Range(0.00f, 70.00f)] private float maxAscendableSlopeAngle = 45.00f;

        [Tooltip("At what distance from ground do we consider the character no longer grounded?")]
        [SerializeField] [Range(0.25f, 25.00f)] private float maxToleratedDistanceFromGround = 0.30f;


        [Header("Surface Alignment Sensitives (Tolerances for Jitter Reduction)")]

        [Tooltip("Should we rotate to align perpendicular to the surface normal?")]
        [SerializeField] private bool maintainPerpendicularityToSurface = true;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [SerializeField] [Range(0.00f, 1.00f)] private float surfaceAlignmentRotationalStrength = 0.10f;

        [Tooltip("At what degrees between up axis and surface normal is considered to be misaligned?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float degreesFromSurfaceNormalThreshold = 0.01f;


        [Header("Movement Sensitives (Tolerances for Jitter Reduction)")]

        [Tooltip("Should we automatically lock movement axes when velocities are within thresholds?")]
        [SerializeField] private bool enableAutomaticAxisLockingForSmallVelocities = true;

        [Tooltip("What linear velocities (in x or y units) do we consider to be small enough to ignore?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float linearVelocityThreshold = 5.00f;

        [Tooltip("What angular velocities (in degrees) do we consider to be small enough to ignore?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float angularVelocityThreshold = 5.00f;
    }
}

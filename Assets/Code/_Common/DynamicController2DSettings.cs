using UnityEngine;


namespace PQ.Common
{
    [CreateAssetMenu(
        fileName = "CharacterControllerSettings",
        menuName = "ScriptableObjects/CharacterController2DSettings_old",
        order    = 1)]
    public class DynamicController2DSettings : ScriptableObject
    {
        public float LocomotionBlendStep                          => _locomotionBlendStep;
        public float JumpStrength                                 => _jumpStrength;
        public float JumpAngle                                    => _jumpAngle;

        public float HorizontalMovementPeakSpeed                  => _horizontalMovementPeakSpeed;
        public float MaxAscendableSlopeAngle                      => _maxAscendableSlopeAngle;
        public float MaxToleratedDistanceFromGround               => _maxToleratedDistanceFromGround;

        public bool  MaintainPerpendicularityToSurface            => _maintainPerpendicularityToSurface;
        public float SurfaceAlignmentRotationalStrength           => _surfaceAlignmentRotationalStrength;
        public float DegreesFromSurfaceNormalThreshold            => _degreesFromSurfaceNormalThreshold;

        public bool  EnableAutomaticAxisLockingForSmallVelocities => _enableAutomaticAxisLockingForSmallVelocities;
        public float LinearVelocityThreshold                      => _linearVelocityThreshold;
        public float AngularVelocityThreshold                     => _angularVelocityThreshold;


        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
                 "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)] [SerializeField] private float _locomotionBlendStep = 0.10f;

        [Header("Walk Settings")]
        [Tooltip("Step size used to move the character")]
        [Range(1, 1000)][SerializeField] private float _horizontalMovementPeakSpeed = 100f;

        // todo: once we fully switch over to kinematic physics, then just give a height, distance to apex
        [Header("Jump Settings")]
        [Tooltip("Strength of jump force in newtons")]
        [Range(25000, 250000)] [SerializeField] private float _jumpStrength = 50000f;

        [Tooltip("Angle to jump (in degrees counterclockwise to the character's forward facing direction)")]
        [Range(0, 90)] [SerializeField] private float _jumpAngle = 45f;


        //[Header("Movement Settings")]
        //[Range(0.50f, 100.00f)] [SerializeField] private float _maxInputSpeed = 10.0f;


        [Header("Surface Handling Ranges")]

        [Tooltip("At what slope angle do we allow the character to walk up to?")]
        [SerializeField] [Range(0.00f, 70.00f)] private float _maxAscendableSlopeAngle = 45.00f;

        [Tooltip("At what distance from ground do we consider the character no longer grounded?")]
        [SerializeField] [Range(0.25f, 25.00f)] private float _maxToleratedDistanceFromGround = 0.30f;


        [Header("Surface Alignment Sensitives (Tolerances for Jitter Reduction)")]

        [Tooltip("Should we rotate to align perpendicular to the surface normal?")]
        [SerializeField] private bool _maintainPerpendicularityToSurface = true;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [SerializeField] [Range(0.00f, 1.00f)] private float _surfaceAlignmentRotationalStrength = 0.10f;

        [Tooltip("At what degrees between up axis and surface normal is considered to be misaligned?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float _degreesFromSurfaceNormalThreshold = 0.01f;


        [Header("Movement Sensitives (Tolerances for Jitter Reduction)")]

        [Tooltip("Should we automatically lock movement axes when velocities are within thresholds?")]
        [SerializeField] private bool _enableAutomaticAxisLockingForSmallVelocities = true;

        [Tooltip("What linear velocities (in x or y units) do we consider to be small enough to ignore?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float _linearVelocityThreshold = 5.00f;

        [Tooltip("What angular velocities (in degrees) do we consider to be small enough to ignore?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float _angularVelocityThreshold = 5.00f;
    }
}

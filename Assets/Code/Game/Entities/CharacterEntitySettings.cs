using UnityEngine;


namespace PQ.Game.Entities
{
    [CreateAssetMenu(
        fileName = "Character2DSettings",
        menuName = "ScriptableObjects/Character2DSettings",
        order    = 1)]
    public class CharacterEntitySettings : ScriptableObject
    {
        public float   LocomotionBlendStep                => _locomotionBlendStep;
        public float   GravityStrength                    => _gravityStrength;
        public Vector2 JumpDistanceToPeak                 => new(_jumpLengthToApex, _jumpHeightToApex);

        public float   HorizontalMovementPeakSpeed        => _horizontalMovementPeakSpeed;
        public float   MaxAscendableSlopeAngle            => _maxAscendableSlopeAngle;
        public float   MaxToleratedDistanceFromGround     => _maxToleratedDistanceFromGround;

        public bool    MaintainPerpendicularityToSurface  => _maintainPerpendicularityToSurface;
        public float   SurfaceAlignmentRotationalStrength => _surfaceAlignmentRotationalStrength;
        public float   DegreesFromSurfaceNormalThreshold  => _degreesFromSurfaceNormalThreshold;


        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
                 "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)] [SerializeField] private float _locomotionBlendStep = 0.10f;

        [Header("Walk Settings")]
        [Tooltip("Step size used to move the character")]
        [Range(1, 1000)][SerializeField] private float _horizontalMovementPeakSpeed = 100f;

        [Header("Jump Settings")]
        [Tooltip("Horizontal distance from jump origin to bottom center of arc")]
        [Range(0, 100)][SerializeField] private float _jumpLengthToApex = 10f;

        [Tooltip("Vertical Distance from jump origin to top of arc")]
        [Range(0, 100)][SerializeField] private float _jumpHeightToApex = 10f;

        [Tooltip("How strong is the pull of gravity?")]
        [Range(0, 100)][SerializeField] private float _gravityStrength = 10f;


        [Header("Surface Handling Ranges")]

        [Tooltip("At what slope angle do we allow the character to walk up to?")]
        [SerializeField] [Range(0.00f, 70.00f)] private float _maxAscendableSlopeAngle = 45f;

        [Tooltip("At what distance from ground do we consider the character no longer grounded?")]
        [SerializeField] [Range(0.25f, 25.00f)] private float _maxToleratedDistanceFromGround = 0.30f;


        [Header("Surface Alignment Sensitives (Tolerances for Jitter Reduction)")]

        [Tooltip("Should we rotate to align perpendicular to the surface normal?")]
        [SerializeField] private bool _maintainPerpendicularityToSurface = true;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [SerializeField] [Range(0.00f, 1.00f)] private float _surfaceAlignmentRotationalStrength = 0.10f;

        [Tooltip("At what degrees between up axis and surface normal is considered to be misaligned?")]
        [SerializeField] [Range(0.01f, 20.00f)] private float _degreesFromSurfaceNormalThreshold = 0.01f;
    }
}

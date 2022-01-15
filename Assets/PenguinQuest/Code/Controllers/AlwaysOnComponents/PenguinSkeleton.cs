using UnityEngine;


namespace PenguinQuest.Controllers
{
    [System.Flags]
    public enum PenguinColliderConstraints
    {
        None            = 0,
        DisableHead     = 1 << 1,
        DisableTorso    = 1 << 2,
        DisableFlippers = 1 << 3,
        DisableFeet     = 1 << 4,
        DisableAll      = ~0,
    }

    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("PenguinSkeleton")]
    public class PenguinSkeleton : MonoBehaviour
    {
        [Header("Collider Constraints")]
        [SerializeField] private PenguinColliderConstraints colliderConstraints = PenguinColliderConstraints.None;

        [Header("Collider References")]
        [SerializeField] private CapsuleCollider2D headCollider              = default;
        [SerializeField] private CapsuleCollider2D torsoCollider             = default;
        [SerializeField] private CapsuleCollider2D frontFlipperUpperCollider = default;
        [SerializeField] private CapsuleCollider2D frontFlipperLowerCollider = default;
        [SerializeField] private CapsuleCollider2D frontFootCollider         = default;
        [SerializeField] private CapsuleCollider2D backFootCollider          = default;
        
        public Collider2D ColliderHead              => headCollider;
        public Collider2D ColliderTorso             => torsoCollider;
        public Collider2D ColliderFrontFlipperUpper => frontFlipperUpperCollider;
        public Collider2D ColliderFrontFlipperLower => frontFlipperLowerCollider;
        public Collider2D ColliderFrontFoot         => frontFootCollider;
        public Collider2D ColliderBackFoot          => backFootCollider;
        
        public PenguinColliderConstraints ColliderConstraints
        {
            get
            {
                // synchronize the mask to reflect any external changes made to collider enability,
                // for example, if the upper/lower flipper colliders were disabled, then we set the DisableFlippers flag
                colliderConstraints = GetConstraintsAccordingToDisabledColliders();
                return colliderConstraints;
            }
            set
            {
                if (colliderConstraints != value)
                {
                    Debug.Log($"PenguinSkeleton: ColliderConstraints.set: " +
                              $"Changing constraints from {colliderConstraints} to {value}");
                }
                colliderConstraints = value;
                UpdateColliderEnabilityAccordingToConstraints();
            }
        }
        
        void Start()
        {
            // initialize using inspector values on start - instead of OnAwake since our class is a (always active) component
            ColliderConstraints = colliderConstraints;
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            ColliderConstraints = colliderConstraints;
        }
        #endif

        private void UpdateColliderEnabilityAccordingToConstraints()
        {
            ColliderHead             .enabled = !HasAllFlags(PenguinColliderConstraints.DisableHead);
            ColliderTorso            .enabled = !HasAllFlags(PenguinColliderConstraints.DisableTorso);
            ColliderFrontFlipperUpper.enabled = !HasAllFlags(PenguinColliderConstraints.DisableFlippers);
            ColliderFrontFlipperLower.enabled = !HasAllFlags(PenguinColliderConstraints.DisableFlippers);
            ColliderFrontFoot        .enabled = !HasAllFlags(PenguinColliderConstraints.DisableFeet);
            ColliderBackFoot         .enabled = !HasAllFlags(PenguinColliderConstraints.DisableFeet);
        }

        private PenguinColliderConstraints GetConstraintsAccordingToDisabledColliders()
        {
            // note that for any flag to be set, _all_ corresponding colliders must be disabled
            PenguinColliderConstraints constraints = PenguinColliderConstraints.None;
            if (!headCollider.enabled)
            {
                constraints |= PenguinColliderConstraints.DisableHead;
            }
            if (!torsoCollider.enabled)
            {
                constraints |= PenguinColliderConstraints.DisableTorso;
            }
            if (!frontFlipperUpperCollider.enabled && !frontFlipperLowerCollider.enabled)
            {
                constraints |= PenguinColliderConstraints.DisableFlippers;
            }
            if (!frontFootCollider.enabled && !backFootCollider.enabled)
            {
                constraints |= PenguinColliderConstraints.DisableFeet;
            }
            return constraints;
        }

        // do the constraints contain all given flags?
        private bool HasAllFlags(PenguinColliderConstraints flags)
        {
            return (colliderConstraints & flags) == flags;
        }
    }
}

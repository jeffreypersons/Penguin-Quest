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

    [System.Serializable]
    [AddComponentMenu("Penguin Skeleton")]
    public class PenguinSkeleton : MonoBehaviour
    {
        [Header("Collider Constraints")]
        [SerializeField] private PenguinColliderConstraints _constraints = PenguinColliderConstraints.None;

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
                // the colliders may have been enabled/disabled individually elsewhere, so synchronize the mask
                _constraints = GetConstraintsAccordingToDisabledColliders();
                return _constraints;
            }
            set
            {
                // ignore all other flags if none is selected, and if none is not selected then use all overrides everything else
                _constraints = value;
                if (_constraints.HasFlag(PenguinColliderConstraints.None))
                {
                    _constraints = PenguinColliderConstraints.None;
                }
                else if (_constraints.HasFlag(PenguinColliderConstraints.DisableAll))
                {
                    _constraints = PenguinColliderConstraints.DisableAll;
                }
                UpdateColliderEnabilityAccordingToConstraints();
            }
        }

        private bool IsFlagSet(PenguinColliderConstraints flag)
        {
            return (_constraints & flag) == flag;
        }
        private void UpdateColliderEnabilityAccordingToConstraints()
        {
            ColliderHead             .enabled = !IsFlagSet(PenguinColliderConstraints.DisableHead);
            ColliderTorso            .enabled = !IsFlagSet(PenguinColliderConstraints.DisableTorso);
            ColliderFrontFlipperUpper.enabled = !IsFlagSet(PenguinColliderConstraints.DisableFlippers);
            ColliderFrontFlipperLower.enabled = !IsFlagSet(PenguinColliderConstraints.DisableFlippers);
            ColliderFrontFoot        .enabled = !IsFlagSet(PenguinColliderConstraints.DisableFeet);
            ColliderBackFoot         .enabled = !IsFlagSet(PenguinColliderConstraints.DisableFeet);
        }

        private PenguinColliderConstraints GetConstraintsAccordingToDisabledColliders()
        {
            // note that if the field corresponds to more than one collider, all must be disabled
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
    }
}

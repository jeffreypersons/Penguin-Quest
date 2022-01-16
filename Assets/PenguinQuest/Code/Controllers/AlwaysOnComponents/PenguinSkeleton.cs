using UnityEngine;


namespace PenguinQuest.Controllers
{
    [System.Flags]
    public enum PenguinColliderConstraints
    {
        None               = 0,
        DisableHead        = 1 << 1,
        DisableTorso       = 1 << 2,
        DisableFlippers    = 1 << 3,
        DisableFeet        = 1 << 4,
        DisableBoundingBox = 1 << 5,
        DisableAll         = ~0,
    }

    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("PenguinSkeleton")]
    public class PenguinSkeleton : MonoBehaviour
    {
        [Header("Body Part Collider Constraints")]
        [SerializeField] private PenguinColliderConstraints colliderConstraints = PenguinColliderConstraints.DisableBoundingBox;

        [Header("Collider References")]
        [SerializeField] private BoxCollider2D     boundingBox               = default;
        [SerializeField] private CapsuleCollider2D headCollider              = default;
        [SerializeField] private CapsuleCollider2D torsoCollider             = default;
        [SerializeField] private CapsuleCollider2D frontFlipperUpperCollider = default;
        [SerializeField] private CapsuleCollider2D frontFlipperLowerCollider = default;
        [SerializeField] private CapsuleCollider2D frontFootCollider         = default;
        [SerializeField] private CapsuleCollider2D backFootCollider          = default;

        public Collider2D ColliderBoundingBox       => boundingBox;
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
                UpdateColliderConstraints();
                return colliderConstraints;
            }
            set
            {
                // override whatever constraints and collider enability was before with our new constraints
                colliderConstraints = value;
                UpdateColliderConstraints();
            }
        }
        
        private PenguinColliderConstraints? _previousConstraints = null;

        void Update()
        {
            UpdateColliderConstraints();
        }
        
        private void UpdateColliderConstraints()
        {
            PenguinColliderConstraints inspectorConstraints = colliderConstraints;
            PenguinColliderConstraints actualConstraints    = GetConstraintsAccordingToDisabledColliders();

            // if first time entering since a recompile, then force to whatever set in the inspector's constraints field
            if (_previousConstraints == null)
            {
                colliderConstraints = inspectorConstraints;
            }
            // otherwise if our inspector field changed we want it to override whatever our constraints are
            else if (inspectorConstraints != _previousConstraints)
            {
                colliderConstraints = inspectorConstraints;
            }
            // otherwise our inspector field is unchanged, so reflect any external changes made to collider enability
            else if (actualConstraints != _previousConstraints)
            {
                colliderConstraints = actualConstraints;
            }
            // if nothing has unchanged then there is no need to update, so terminate early
            else
            {
                return;
            }

            UpdateColliderEnabilityAccordingToConstraints(colliderConstraints);
            colliderConstraints = GetConstraintsAccordingToDisabledColliders();

            Debug.Log($"PenguinSkeleton: SetColliderConstraints: Overriding constraints " +
                      $"from {(_previousConstraints == null? "<uninitialized>" :_previousConstraints)} " +
                      $"to {colliderConstraints}");

            _previousConstraints = colliderConstraints;
        }

        private void UpdateColliderEnabilityAccordingToConstraints(PenguinColliderConstraints constraints)
        {
            ColliderHead             .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableHead);
            ColliderTorso            .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableTorso);
            ColliderFrontFlipperUpper.enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFlippers);
            ColliderFrontFlipperLower.enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFlippers);
            ColliderFrontFoot        .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFeet);
            ColliderBackFoot         .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFeet);
            ColliderBoundingBox      .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableBoundingBox);
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
            if (!boundingBox.enabled)
            {
                constraints |= PenguinColliderConstraints.DisableBoundingBox;
            }
            return constraints;
        }

        // do the constraints contain all given flags?
        private static bool HasAllFlags(PenguinColliderConstraints constraints, PenguinColliderConstraints flags)
        {
            return (constraints & flags) == flags;
        }
    }
}

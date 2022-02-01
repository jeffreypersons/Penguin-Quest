using UnityEngine;
using PenguinQuest.Utils;


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


    /*
    Collection of penguin components.

    Serves as the collection of the components that make up a penguin in our game,
    including rigidbody, animator, and colliders, with centralized support for changing mass,
    disabling/enabling colliders, etc.

    The purpose in all this is to centralize component and child object access, so that other
    scripts don't have to worry about caching
    */
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("PenguinEntity")]
    public class PenguinEntity : MonoBehaviour
    {
        [Header("Mass Settings")]
        [Tooltip("Constant (fixed) total mass for rigidbody")]
        [Range(50, 5000)] [SerializeField] private float mass = 500;

        [Tooltip("Center of mass x component relative to skeletal root (ie smaller x means more prone to fall backwards)")]
        [Range(-500.00f, 500.00f)] [SerializeField] private float centerOfMassX = 0.00f;

        [Tooltip("Center of mass y component relative to skeletal root (ie smaller y means more resistant to falling over)")]
        [Range(-500.00f, 500.00f)] [SerializeField] private float centerOfMassY = 0.00f;


        [Header("Body Part Collider Constraints")]
        [SerializeField] private PenguinColliderConstraints colliderConstraints = PenguinColliderConstraints.DisableBoundingBox;

        
        [Header("Component References")]
        [SerializeField] private PenguinAnimation  penguinAnimation;
        [SerializeField] private Rigidbody2D       penguinRigidbody;

        [Header("Collider References")]
        [SerializeField] private BoxCollider2D     boundingBoxCollider       = default;
        [SerializeField] private CapsuleCollider2D headCollider              = default;
        [SerializeField] private CapsuleCollider2D torsoCollider             = default;
        [SerializeField] private CapsuleCollider2D frontFlipperUpperCollider = default;
        [SerializeField] private CapsuleCollider2D frontFlipperLowerCollider = default;
        [SerializeField] private CapsuleCollider2D frontFootCollider         = default;
        [SerializeField] private CapsuleCollider2D backFootCollider          = default;


        public PenguinAnimation Animation            => penguinAnimation;
        public Rigidbody2D      Rigidbody            => penguinRigidbody;
        public Vector2          SkeletalRootPosition => penguinAnimation.SkeletalRootPosition;
        public Vector2          CenterOfMass         => penguinRigidbody.worldCenterOfMass;


        public BoxCollider2D ColliderBoundingBox    => boundingBoxCollider;
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
        
        void Start()
        {
            colliderConstraints = GetConstraintsAccordingToDisabledColliders();
        }

        void Update()
        {
            UpdateColliderConstraints();
        }
        
        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Rigidbody.useAutoMass)
            {
                return;
            }
            if (!Mathf.Approximately(centerOfMassX, Rigidbody.centerOfMass.x) ||
                !Mathf.Approximately(centerOfMassY, Rigidbody.centerOfMass.y))
            {
                penguinRigidbody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);
            }
            if (!Mathf.Approximately(mass, penguinRigidbody.mass))
            {
                penguinRigidbody.mass = mass;
            }
        }

        void OnDrawGizmos()
        {
            GizmosUtils.DrawSphere(SkeletalRootPosition, 1.00f, Color.white);
            GizmosUtils.DrawSphere(CenterOfMass,         2.00f, Color.red);
        }
        #endif
        
        private PenguinColliderConstraints? _previousConstraints = null;

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

            if (_previousConstraints != null)
            {
                Debug.Log($"PenguinSkeleton: SetColliderConstraints: Overriding constraints " +
                          $"from {_previousConstraints} to {colliderConstraints}");
            }
            _previousConstraints = colliderConstraints;
        }

        private void UpdateColliderEnabilityAccordingToConstraints(PenguinColliderConstraints constraints)
        {
            headCollider             .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableHead);
            torsoCollider            .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableTorso);
            frontFlipperUpperCollider.enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFlippers);
            frontFlipperLowerCollider.enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFlippers);
            frontFootCollider        .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFeet);
            backFootCollider         .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFeet);
            boundingBoxCollider      .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableBoundingBox);
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
            if (!boundingBoxCollider.enabled)
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

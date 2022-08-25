using System.Diagnostics.Contracts;
using UnityEngine;
using PQ.Common;
using PQ.Common.Extensions;


namespace PQ.Entities.Penguin
{
    /*
    Collection of penguin components.

    Serves as the collection of the components that make up a penguin in our game,
    including rigidbody, animator, and colliders, with centralized support for changing mass,
    disabling/enabling colliders, etc.

    The purpose in all this is to centralize component and child object access, so that other
    scripts don't have to worry about caching.

    Note that this always is running, so that gizmos and editor scripts can reference any of its properties
    without worry about when things are valid and active.
    */
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("PenguinBlob")]
    public class PenguinBlob : MonoBehaviour
    {
        [Header("Body Part Collider Constraints")]
        [SerializeField] private PenguinColliderConstraints _colliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        
        [Header("Setting Bundles")]
        [SerializeField] private KinematicCharacter2DSettings _penguinOnFeetSettings;
        [SerializeField] private KinematicCharacter2DSettings _penguinOnBellySettings;

        [Header("Component References")]
        [SerializeField] private PenguinAnimation     _penguinAnimation;
        [SerializeField] private KinematicCharacter2D _characterController;

        [Header("Collider References")]
        [SerializeField] private BoxCollider2D     _boundingBoxCollider;
        [SerializeField] private CapsuleCollider2D _headCollider;
        [SerializeField] private CapsuleCollider2D _torsoCollider;
        [SerializeField] private CapsuleCollider2D _frontFlipperUpperCollider;
        [SerializeField] private CapsuleCollider2D _frontFlipperLowerCollider;
        [SerializeField] private CapsuleCollider2D _frontFootCollider;
        [SerializeField] private CapsuleCollider2D _backFootCollider;
        
        
        public KinematicCharacter2DSettings OnFeetSettings  => _penguinOnFeetSettings;
        public KinematicCharacter2DSettings OnBellySettings => _penguinOnBellySettings;

        public PenguinAnimation     Animation            => _penguinAnimation;
        public KinematicCharacter2D CharacterController  => _characterController;
        public Vector2              SkeletalRootPosition => _penguinAnimation.SkeletalRootPosition;

        public BoxCollider2D ColliderBoundingBox       => _boundingBoxCollider;
        public Collider2D    ColliderHead              => _headCollider;
        public Collider2D    ColliderTorso             => _torsoCollider;
        public Collider2D    ColliderFrontFlipperUpper => _frontFlipperUpperCollider;
        public Collider2D    ColliderFrontFlipperLower => _frontFlipperLowerCollider;
        public Collider2D    ColliderFrontFoot         => _frontFootCollider;
        public Collider2D    ColliderBackFoot          => _backFootCollider;

        public PenguinColliderConstraints ColliderConstraints
        {
            get
            {
                // synchronize the mask to reflect any external changes made to collider enability,
                // for example, if the upper/lower flipper colliders were disabled, then we set the DisableFlippers flag
                UpdateColliderConstraints();
                return _colliderConstraints;
            }
            set
            {
                // override whatever constraints and collider enability was before with our new constraints
                _colliderConstraints = value;
                UpdateColliderConstraints();
            }
        }

        public void ReadjustBoundingBox(Vector2 offset, Vector2 size, float edgeRadius)
        {
            bool wasPreviouslyEnabled  = _boundingBoxCollider.enabled;
            Vector2 previousOffset     = _boundingBoxCollider.offset;
            Vector2 previousSize       = _boundingBoxCollider.size;
            float   previousEdgeRadius = _boundingBoxCollider.edgeRadius;

            _boundingBoxCollider.enabled    = true;
            _boundingBoxCollider.offset     = offset;
            _boundingBoxCollider.size       = size;
            _boundingBoxCollider.edgeRadius = edgeRadius;

            Debug.Log($"ReadjustBoundingBox : Changed bounding box " +
                      $"from {{offset={previousOffset}, size={previousSize}, edge_radius={previousEdgeRadius}}} " +
                      $"to {{offset={offset}, size={size}, edge_radius={edgeRadius}}}");

            _boundingBoxCollider.enabled = wasPreviouslyEnabled;
        }

        void Start()
        {
            _colliderConstraints = GetConstraintsAccordingToDisabledColliders();
        }

        void Update()
        {
            UpdateColliderConstraints();
        }
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            GizmoExtensions.DrawSphere(_penguinAnimation.SkeletalRootPosition, 1.00f, Color.white);
        }
        #endif
        
        private PenguinColliderConstraints? _previousConstraints = null;

        private void UpdateColliderConstraints()
        {
            PenguinColliderConstraints inspectorConstraints = _colliderConstraints;
            PenguinColliderConstraints actualConstraints    = GetConstraintsAccordingToDisabledColliders();

            // if first time entering since a recompile, then force to whatever set in the inspector's constraints field
            if (_previousConstraints == null)
            {
                _colliderConstraints = inspectorConstraints;
            }
            // otherwise if our inspector field changed we want it to override whatever our constraints are
            else if (inspectorConstraints != _previousConstraints)
            {
                _colliderConstraints = inspectorConstraints;
            }
            // otherwise our inspector field is unchanged, so reflect any external changes made to collider enability
            else if (actualConstraints != _previousConstraints)
            {
                _colliderConstraints = actualConstraints;
            }
            // if nothing has unchanged then there is no need to update, so terminate early
            else
            {
                return;
            }

            UpdateColliderEnabilityAccordingToConstraints(_colliderConstraints);
            _colliderConstraints = GetConstraintsAccordingToDisabledColliders();

            if (_previousConstraints != null)
            {
                Debug.Log($"Overriding constraints from {{{_previousConstraints}}} to {{{_colliderConstraints}}}");
            }
            _previousConstraints = _colliderConstraints;
        }


        private void UpdateColliderEnabilityAccordingToConstraints(PenguinColliderConstraints constraints)
        {
            _headCollider             .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableHead);
            _torsoCollider            .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableTorso);
            _frontFlipperUpperCollider.enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFlippers);
            _frontFlipperLowerCollider.enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFlippers);
            _frontFootCollider        .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFeet);
            _backFootCollider         .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableFeet);
            _boundingBoxCollider      .enabled = !HasAllFlags(constraints, PenguinColliderConstraints.DisableBoundingBox);
        }

        private PenguinColliderConstraints GetConstraintsAccordingToDisabledColliders()
        {
            // note that for any flag to be set, _all_ corresponding colliders must be disabled
            PenguinColliderConstraints constraints = PenguinColliderConstraints.None;
            if (IsDisabled(_headCollider))
            {
                constraints |= PenguinColliderConstraints.DisableHead;
            }
            if (IsDisabled(_torsoCollider))
            {
                constraints |= PenguinColliderConstraints.DisableTorso;
            }
            if (IsDisabled(_frontFlipperUpperCollider) && IsDisabled(_frontFlipperLowerCollider))
            {
                constraints |= PenguinColliderConstraints.DisableFlippers;
            }
            if (IsDisabled(_frontFootCollider) && IsDisabled(_backFootCollider))
            {
                constraints |= PenguinColliderConstraints.DisableFeet;
            }
            if (IsDisabled(_boundingBoxCollider))
            {
                constraints |= PenguinColliderConstraints.DisableBoundingBox;
            }
            return constraints;
        }


        [Pure]
        private static bool IsDisabled(Collider2D collider)
        {
            return !collider.enabled;
        }

        [Pure]
        private static bool HasAllFlags(PenguinColliderConstraints constraints, PenguinColliderConstraints flags)
        {
            // check if ALL given flags are a proper subset of constraints
            return (constraints & flags) == flags;
        }
    }
}

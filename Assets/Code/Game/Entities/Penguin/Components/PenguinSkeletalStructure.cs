using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.Game.Entities.Penguin
{
    /*
    Collection of penguin components, such as colliders and bones.

    This is intended to keep all that skeletal animation sort of book-keeping code out of the core game logic.
    */
    public class PenguinSkeletalStructure : MonoBehaviour
    {
        [Header("Body Part Collider Constraints")]
        [SerializeField] private PenguinColliderConstraints _colliderConstraints = PenguinColliderConstraints.None;

        [Header("Collider References")]
        [SerializeField] private BoxCollider2D     _outerCollider;
        [SerializeField] private CapsuleCollider2D _headCollider;
        [SerializeField] private CapsuleCollider2D _torsoCollider;
        [SerializeField] private CapsuleCollider2D _frontFlipperUpperCollider;
        [SerializeField] private CapsuleCollider2D _frontFlipperLowerCollider;
        [SerializeField] private CapsuleCollider2D _frontFootCollider;
        [SerializeField] private CapsuleCollider2D _backFootCollider;

        public BoxCollider2D     OuterCollider             => _outerCollider;
        public CapsuleCollider2D ColliderHead              => _headCollider;
        public CapsuleCollider2D ColliderTorso             => _torsoCollider;
        public CapsuleCollider2D ColliderFrontFlipperUpper => _frontFlipperUpperCollider;
        public CapsuleCollider2D ColliderFrontFlipperLower => _frontFlipperLowerCollider;
        public CapsuleCollider2D ColliderFrontFoot         => _frontFootCollider;
        public CapsuleCollider2D ColliderBackFoot          => _backFootCollider;

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
            bool wasPreviouslyEnabled = _outerCollider.enabled;
            Vector2 previousOffset    = _outerCollider.offset;
            Vector2 previousSize      = _outerCollider.size;

            _outerCollider.enabled = true;
            _outerCollider.offset  = offset;
            _outerCollider.size    = size;

            Debug.Log($"ReadjustBoundingBox : Changed bounding box " +
                      $"from {{offset={previousOffset}, size={previousSize}}} " +
                      $"to {{offset={offset}, size={size}, edge_radius={edgeRadius}}}");

            _outerCollider.enabled = wasPreviouslyEnabled;
        }

        void Awake()
        {
            _colliderConstraints = GetConstraintsAccordingToDisabledColliders();
        }
        
        void Update()
        {
            UpdateColliderConstraints();
        }

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
            // todo: replace with enum set..
            _headCollider             .enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableHead);
            _torsoCollider            .enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableTorso);
            _frontFlipperUpperCollider.enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableFlippers);
            _frontFlipperLowerCollider.enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableFlippers);
            _frontFootCollider        .enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableFeet);
            _backFootCollider         .enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableFeet);
            _outerCollider            .enabled = !IsDisabled(constraints, PenguinColliderConstraints.DisableOuter);
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
            if (IsDisabled(_outerCollider))
            {
                constraints |= PenguinColliderConstraints.DisableOuter;
            }
            return constraints;
        }


        [Pure]
        private static bool IsDisabled(Collider2D collider)
        {
            return collider == null || !collider || !collider.enabled;
        }

        [Pure]
        private static bool IsDisabled(PenguinColliderConstraints constraints, PenguinColliderConstraints flags)
        {
            // check if ALL given flags are a proper subset of constraints
            // note that unlike enum.hasFlags, this returns false for None = 0
            return (constraints & flags) == flags;
        }
    }
}

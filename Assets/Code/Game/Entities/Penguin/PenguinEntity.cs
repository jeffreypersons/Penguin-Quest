using System;
using UnityEngine;
using PQ.Common.Fsm;
using PQ.Common.Physics;
using PQ.Common.Extensions;
using UnityEditor;


namespace PQ.Game.Entities.Penguin
{
    /*
    Collection of penguin components.

    Serves as the collection of the components that make up a penguin in our game,
    including rigidbody, animator, and colliders, etc.

    The purpose in all this is to centralize component and child object access, so that other
    scripts don't have to worry about caching.

    Note that this always is running, so that gizmos and editor scripts can reference any of its properties
    without worry about when things are valid and active.
    */
    public sealed class PenguinEntity : FsmSharedData
    {
        [Header("Component/Config References")]
        [SerializeField] private PhysicsBody2D            _physicsBody;
        [SerializeField] private PenguinAnimationDriver   _penguinAnimation;
        [SerializeField] private PenguinSkeletalStructure _penguinSkeleton;
        [SerializeField] private PenguinFsmDriver         _penguinStateMachine;
        [SerializeField] private PenguinTuningConfig      _config;

        
        // todo: find a better way to hook it up
        public GameEventCenter EventBus { get; set; }
        public PhysicsBody2D            PhysicsBody  => _physicsBody;
        public PenguinTuningConfig      Config       => _config;
        public PenguinAnimationDriver   Animation    => _penguinAnimation;
        public PenguinSkeletalStructure Skeleton     => _penguinSkeleton;
        public PenguinFsmDriver         StateMachine => _penguinStateMachine;

        // todo: add acceleration curves here until we find a better place to put it


        #if UNITY_EDITOR
        void Start()
        {
            if (EditorApplication.isPlaying && EventBus == null)
            {
                throw new NullReferenceException("Event bus of penguin blob is disconnected - expected assignment prior to start");
            }
        }

        void OnValidate()
        {
            _config.OnChanged.AddHandler(AdjustBoundsToMatchConfig);
        }

        void OnDestroy()
        {
            _config.OnChanged.RemoveHandler(AdjustBoundsToMatchConfig);
        }


        void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying)
            {
                VisualExtensions.Gizmos.Duration = Time.fixedDeltaTime;
                VisualExtensions.Gizmos.DrawCircle(_penguinAnimation.SkeletalRootPosition, 0.025f, Color.white);
            }
        }


        private void AdjustBoundsToMatchConfig()
        {
            // avoid updating with inspector if loading the original prefab from disk (which occurs before loading the instance)
            // otherwise the default inspector values are used. By skipping persistent objects, we effectively only update when values are
            // changed in the inspector
            if (EditorUtility.IsPersistent(this))
            {
                return;
            }

            // todo: add option to switch between prone and upright default poses, and set bounds accordingly
            if (!EditorApplication.isPlaying)
            {
                _physicsBody.SetAABBMinMax(_config.boundsMinUpright, _config.boundsMaxUpright, _config.skinWidthUpright);
            }
        }
        #endif
    }
}

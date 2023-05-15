using System;
using UnityEngine;
using PQ.Common.Fsm;
using PQ.Common.Physics;
using PQ.Common.Extensions;


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
        [SerializeField] private KinematicBody2D          _kinematicBody2D;
        [SerializeField] private PenguinAnimationDriver   _penguinAnimation;
        [SerializeField] private PenguinSkeletalStructure _penguinSkeleton;
        [SerializeField] private PenguinFsmDriver         _penguinStateMachine;
        [SerializeField] private PenguinTuningConfig      _config;

        
        // todo: find a better way to hook it up
        public GameEventCenter EventBus { get; set; }
        public KinematicBody2D          PhysicsBody  => _kinematicBody2D;
        public PenguinTuningConfig      Config       => _config;
        public PenguinAnimationDriver   Animation    => _penguinAnimation;
        public PenguinSkeletalStructure Skeleton     => _penguinSkeleton;
        public PenguinFsmDriver         StateMachine => _penguinStateMachine;

        // todo: add acceleration curves here until we find a better place to put it


        #if UNITY_EDITOR
        void Start()
        {
            if (UnityEditor.EditorApplication.isPlaying && EventBus == null)
            {
                throw new NullReferenceException("Event bus of penguin blob is disconnected - expected assignment prior to start");
            }
        }

        void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                GizmoExtensions.DrawSphere(_penguinAnimation.SkeletalRootPosition, 0.025f, Color.white);
            }
        }
        #endif
    }
}

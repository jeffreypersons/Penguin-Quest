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
        private MovementController2D _characterController;

        [Header("Component References")]
        [SerializeField] private PenguinAnimationDriver   _penguinAnimation;
        [SerializeField] private PenguinSkeletalStructure _penguinSkeleton;
        [SerializeField] private PenguinFsmDriver         _penguinStateMachine;

        [Header("Configs")]
        [SerializeField] [Range(0, 100)] private float _maxWalkSpeed  =  5f;
        [SerializeField] [Range(0, 100)] private float _maxSlideSpeed = 20f;
        [SerializeField] private KinematicBody2DSettings _onFeetSettings;
        [SerializeField] private KinematicBody2DSettings _onBellySettings;

        // todo: add acceleration curves and stuff until we find a better place to put it

        public PenguinAnimationDriver   Animation    => _penguinAnimation;
        public PenguinSkeletalStructure Skeleton     => _penguinSkeleton;
        public MovementController2D     Movement     => _characterController;
        public PenguinFsmDriver         StateMachine => _penguinStateMachine;
        
        public float MaxWalkSpeed  => _maxWalkSpeed;
        public float MaxSlideSpeed => _maxSlideSpeed;

        public KinematicBody2DSettings  FeetSettings  => _onFeetSettings;
        public KinematicBody2DSettings  BellySettings => _onBellySettings;


        // todo: think of a better way of doing this hooking up..
        public GameEventCenter EventBus { get; set; }

        void Awake()
        {
            _characterController = new MovementController2D(gameObject);
        }

        #if UNITY_EDITOR
        void Start()
        {
            if (UnityEditor.EditorApplication.isPlaying && EventBus == null)
            {
                throw new NullReferenceException("Event bus of penguin blob is disconnected");
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

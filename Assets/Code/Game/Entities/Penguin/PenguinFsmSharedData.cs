using System;
using UnityEngine;
using PQ.Common.Fsm;
using PQ.Common.Extensions;


namespace PQ.Game.Entities.Penguin
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
    public sealed class PenguinFsmSharedData : FsmSharedData
    {
        [Header("Setting Bundles")]
        [SerializeField] private PenguinEntitySettings _penguinOnFeetSettings;
        [SerializeField] private PenguinEntitySettings _penguinOnBellySettings;

        [Header("Component References")]
        [SerializeField] private PenguinAnimationDriver   _penguinAnimation;
        [SerializeField] private PenguinSkeletalStructure _penguinSkeleton;

        private PenguinEntity _penguinEntity;

        public PenguinEntitySettings    OnFeetSettings       => _penguinOnFeetSettings;
        public PenguinEntitySettings    OnBellySettings      => _penguinOnBellySettings;

        public PenguinAnimationDriver   Animation            => _penguinAnimation;
        public PenguinSkeletalStructure Skeleton             => _penguinSkeleton;
        public PenguinEntity            CharacterController  => _penguinEntity;
        public Vector2                  SkeletalRootPosition => _penguinAnimation.SkeletalRootPosition;


        // todo: think of a better way of doing this hooking up..
        public GameEventCenter EventBus { get; set; }

        void Awake()
        {
            _penguinEntity = new PenguinEntity(gameObject);
        }

        void Start()
        {
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }

            if (EventBus == null)
            {
                throw new NullReferenceException("Caution: Event bus of penguin blob is disconnected");
            }
            #endif
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }

            GizmoExtensions.DrawSphere(_penguinAnimation.SkeletalRootPosition, 0.025f, Color.white);
        }
        #endif
    }
}

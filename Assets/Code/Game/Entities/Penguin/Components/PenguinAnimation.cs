using System;
using UnityEngine;
using PQ.Common.Events;
using PQ.Common.Containers;


namespace PQ.Game.Entities.Penguin
{
    /*
    Overview
    --------
    Component for listening to animation events, interfacing with animator and setting animator parameters.

    Intended to be attached at the playerModel level.

    Note that animation clip event names and method names in this class must match exactly in order to receive events
    configured in the animation clips.
    */
    [ExecuteAlways]
    [Serializable]
    public class PenguinAnimation : MonoBehaviour
    {
        // rather than hold a bunch of properties, we use these IDs for looking up which events to trigger (from animator),
        // or listen to (from client code), greatly consolidating animation boilerplate
        public enum EventId
        {
            JumpLiftOff,
            LieDownStarted,
            LieDownMidpoint,
            LieDownEnded,
            StandUpStarted,
            StandUpEnded,
            Fired,
            Used,
            FrontFootDown,
        }

        /*
        Reminder: These parameters _must_ match the names in mecanim.
        Unfortunately there is no easy way to generate the parameter names,
        so just be careful to make sure that they match the parameters listed in the Unity Animator.
        */
        // todo: look into validation of the param names with the animator using below enums
        public enum Params
        {
            Locomotion,
            SlopeIntensity,
            IsGrounded,
            TriggerLieDown,
            TriggerStandUp,
            TriggerJumpUp,
            TriggerFire,
            TriggerUse,
        }

        private readonly string paramLocomotion = "LocomotionIntensity";
        private readonly string paramSlope      = "SlopeIntensity";
        private readonly string paramIsGrounded = "IsGrounded";
        private readonly string paramLie        = "LieDown";
        private readonly string paramStand      = "StandUp";
        private readonly string paramJump       = "JumpUp";
        private readonly string paramFire       = "Fire";
        private readonly string paramUse        = "Use";


        [Header("Animator Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private bool     _logEvents = false;

        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
         "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)][SerializeField] private float _locomotionBlendStep = 0.10f;

        private EnumMap<EventId, PqEvent> _animationEvents;


        private void Awake()
        {
            _animationEvents = new EnumMap<EventId, PqEvent>();
            foreach (EventId id in _animationEvents.EnumFields)
            {
                _animationEvents.Add(id, new PqEvent(id.ToString()));
            }
            Debug.Log("Populated animation event mapping: " + _animationEvents);
        }
        
        // our callback to hook up with Animator in animation clip window, for triggering our custom events
        private void RaiseEvent(EventId id)
        {
            if (_logEvents)
            {
                var className     = GetType().Name;
                var currentFrame  = Time.frameCount - 1;
                var eventReceived = _animationEvents[id].Name;
                Debug.Log($"{className}[Frame:{currentFrame}] - triggering {eventReceived} from animator");
            }
            _animationEvents[id].Raise();
        }



        // How quickly do we blend locomotion? Note that this does not affect anything in the animator,
        // rather it's an animation related kept here for relevance
        public float   LocomotionBlendStep  => _locomotionBlendStep;
        public Vector2 SkeletalRootPosition => _animator.rootPosition;

        public IPqEventReceiver LookupEvent(EventId id) => _animationEvents[id];

        public void SetParamLocomotionIntensity(float ratio) => _animator.SetFloat(paramLocomotion, ratio);
        public void SetParamSlopeIntensity(float ratio)      => _animator.SetFloat(paramSlope,      ratio);
        public void SetParamIsGrounded(bool value)           => _animator.SetBool(paramIsGrounded,  value);

        public void TriggerParamLieDownParameter()           => _animator.SetTrigger(paramLie);
        public void TriggerParamStandUpParameter()           => _animator.SetTrigger(paramStand);
        public void TriggerParamJumpUpParameter()            => _animator.SetTrigger(paramJump);
        public void TriggerParamFireParameter()              => _animator.SetTrigger(paramFire);
        public void TriggerParamUseParameter()               => _animator.SetTrigger(paramUse);


        // to avoid any queuing of triggers (ie jump will fire 5 times if it was during a jump)
        // we can reset each trigger's corresponding queue, which is typically the desired use case
        //
        // in other words, reset any triggers such that any pending animation events are cleared out to avoid them
        // from firing automatically when the animation state exits
        public static void ResetAllAnimatorTriggers(Animator animator)
        {
            foreach (var trigger in animator.parameters)
            {
                if (trigger.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(trigger.name);
                }
            }
        }
    }
}

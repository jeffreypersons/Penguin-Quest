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
        private EnumMap<PenguinAnimationEventId, PqEvent> _animationEvents;

        public Vector2 SkeletalRootPosition => _animator.rootPosition;

        private void Awake()
        {
            _animationEvents = new EnumMap<PenguinAnimationEventId, PqEvent>();
            foreach (PenguinAnimationEventId id in _animationEvents.EnumFields)
            {
                _animationEvents.Add(id, new PqEvent(id.ToString()));
            }
            Debug.Log("Populated animation event mapping: " + _animationEvents);
        }
        
        // our callback to hook up with Animator in animation clip window, for triggering our custom events
        private void RaiseEvent(PenguinAnimationEventId id)
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

        public IPqEventReceiver LookupEvent(PenguinAnimationEventId id) => _animationEvents[id];

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

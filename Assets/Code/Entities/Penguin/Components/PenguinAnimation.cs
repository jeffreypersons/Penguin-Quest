using System;
using UnityEngine;
using PQ.Common.Events;


namespace PQ.Entities.Penguin
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
        [SerializeField] private Animator _animator;
        [SerializeField] private bool _logEvents = false;

        [Header("Animation Settings")]
        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
         "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)][SerializeField] private float _locomotionBlendStep = 0.10f;

        /*
        Reminder: These parameters _must_ match the names in mecanim.
        Unfortunately there is no easy way to generate the parameter names,
        so just be careful to make sure that they match the parameters listed in the Unity Animator.
        */
        // todo: look into validation of the param names with the animator
        private readonly string paramLocomotion = "LocomotionIntensity";
        private readonly string paramSlope      = "SlopeIntensity";
        private readonly string paramIsGrounded = "IsGrounded";
        private readonly string paramLie        = "LieDown";
        private readonly string paramStand      = "StandUp";
        private readonly string paramJump       = "JumpUp";
        private readonly string paramFire       = "Fire";
        private readonly string paramUse        = "Use";


        // How quickly do we blend locomotion? Note that this does not affect anything in the animator,
        // rather it's an animation related kept here for relevance
        public float LocomotionBlendStep => _locomotionBlendStep;

        private PqEvent _jumpLiftOff     = new("penguin.animation.jump.liftoff");
        private PqEvent _lieDownStarted  = new("penguin.animation.liedown.start");
        private PqEvent _lieDownMidpoint = new("penguin.animation.liedown.mid");
        private PqEvent _lieDownEnded    = new("penguin.animation.liedown.end");
        private PqEvent _standUpStarted  = new("penguin.animation.standup.start");
        private PqEvent _standUpEnded    = new("penguin.animation.standup.end");
        private PqEvent _fired           = new("penguin.animation.fire");
        private PqEvent _used            = new("penguin.animation.use");

        public IPqEventReceiver JumpLiftOff     => _jumpLiftOff;
        public IPqEventReceiver LieDownStarted  => _lieDownStarted;
        public IPqEventReceiver LieDownMidpoint => _lieDownMidpoint;
        public IPqEventReceiver LieDownEnded    => _lieDownEnded;
        public IPqEventReceiver StandUpStarted  => _standUpStarted;
        public IPqEventReceiver StandUpEnded    => _standUpEnded;
        public IPqEventReceiver Fired           => _fired;
        public IPqEventReceiver Used            => _used;

        public void ResetAllTriggers()
        {
            // note that only triggers need to be reset, as they are timing dependent
            // unlike float, int, bool, and string parameters
            _animator.ResetTrigger(paramLie);
            _animator.ResetTrigger(paramStand);
            _animator.ResetTrigger(paramJump);
            _animator.ResetTrigger(paramFire);
            _animator.ResetTrigger(paramUse);
        }

        public Vector2 SkeletalRootPosition => _animator.rootPosition;

        public void SetParamLocomotionIntensity(float ratio) => _animator.SetFloat(paramLocomotion, ratio);
        public void SetParamSlopeIntensity(float ratio)      => _animator.SetFloat(paramSlope,      ratio);
        public void SetParamIsGrounded(bool value)           => _animator.SetBool(paramIsGrounded,  value);
        
        public void TriggerParamLieDownParameter()           => _animator.SetTrigger(paramLie);
        public void TriggerParamStandUpParameter()           => _animator.SetTrigger(paramStand);
        public void TriggerParamJumpUpParameter()            => _animator.SetTrigger(paramJump);
        public void TriggerParamFireParameter()              => _animator.SetTrigger(paramFire);
        public void TriggerParamUseParameter()               => _animator.SetTrigger(paramUse);
        
        
        private void OnLieDownAnimationEventStart()  => ForwardAsEvent(OnLieDownAnimationEventStart,  _lieDownStarted);
        private void OnLieDownAnimationEventMid()    => ForwardAsEvent(OnLieDownAnimationEventMid,    _lieDownMidpoint);
        private void OnLieDownAnimationEventEnd()    => ForwardAsEvent(OnLieDownAnimationEventEnd,    _lieDownEnded);
        
        private void OnStandUpAnimationEventStart()  => ForwardAsEvent(OnStandUpAnimationEventStart,  _standUpStarted);
        private void OnStandUpAnimationEventEnd()    => ForwardAsEvent(OnStandUpAnimationEventEnd,    _standUpEnded);

        private void OnJumpUpAnimationEventImpulse() => ForwardAsEvent(OnJumpUpAnimationEventImpulse, _jumpLiftOff);
        private void OnFireAnimationEvent()          => ForwardAsEvent(OnFireAnimationEvent,          _fired);
        private void OnUseAnimationEvent()           => ForwardAsEvent(OnUseAnimationEvent,           _used);


        private void ForwardAsEvent(Action animatorEvent, PqEvent customEvent)
        {
            if (_logEvents)
            {
                Debug.Log($"[Frame:{Time.frameCount - 1}] " +
                          $"Received {animatorEvent.Method.Name}, forwarding as {customEvent.Name}");
            }

            customEvent.Raise();
        }
    }
}

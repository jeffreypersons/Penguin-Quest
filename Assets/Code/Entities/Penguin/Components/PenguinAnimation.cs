using System;
using UnityEngine;


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

        /*
        Reminder: These parameters _must_ match the names in mecanim.
        Unfortunately there is no easy way to generate the parameter names,
        so just be careful to make sure that they match the parameters listed in the Unity Animator.
        */
        // todo: look into validation of the param names with the animator
        private readonly string paramXMotion    = "XMotionIntensity";
        private readonly string paramYMotion    = "YMotionIntensity";
        private readonly string paramIsGrounded = "IsGrounded";
        private readonly string paramLie        = "LieDown";
        private readonly string paramStand      = "StandUp";
        private readonly string paramJump       = "JumpUp";
        private readonly string paramFire       = "Fire";
        private readonly string paramUse        = "Use";
        
        public event Action JumpLiftOff;
        public event Action LieDownStarted;
        public event Action LieDownMidpoint;
        public event Action LieDownEnded;
        public event Action StandUpStarted;
        public event Action StandUpEnded;
        public event Action Fired;
        public event Action Used;


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

        public void SetParamXMotionIntensity(float ratio) => _animator.SetFloat(paramXMotion, ratio);
        public void SetParamYMotionIntensity(float ratio) => _animator.SetFloat(paramYMotion, ratio);
        public void SetParamIsGrounded(bool value)        => _animator.SetBool(paramIsGrounded, value);
        
        public void TriggerParamLieDownParameter()        => _animator.SetTrigger(paramLie);
        public void TriggerParamStandUpParameter()        => _animator.SetTrigger(paramStand);
        public void TriggerParamJumpUpParameter()         => _animator.SetTrigger(paramJump);
        public void TriggerParamFireParameter()           => _animator.SetTrigger(paramFire);
        public void TriggerParamUseParameter()            => _animator.SetTrigger(paramUse);
        
        
        private void OnLieDownAnimationEventStart()  => ForwardAsEvent(OnLieDownAnimationEventStart,  LieDownStarted);
        private void OnLieDownAnimationEventMid()    => ForwardAsEvent(OnLieDownAnimationEventMid,    LieDownMidpoint);
        private void OnLieDownAnimationEventEnd()    => ForwardAsEvent(OnLieDownAnimationEventEnd,    LieDownEnded);
        
        private void OnStandUpAnimationEventStart()  => ForwardAsEvent(OnStandUpAnimationEventStart,  StandUpStarted);
        private void OnStandUpAnimationEventEnd()    => ForwardAsEvent(OnStandUpAnimationEventEnd,    StandUpEnded);

        private void OnJumpUpAnimationEventImpulse() => ForwardAsEvent(OnJumpUpAnimationEventImpulse, JumpLiftOff);
        private void OnFireAnimationEvent()          => ForwardAsEvent(OnFireAnimationEvent,          Fired);
        private void OnUseAnimationEvent()           => ForwardAsEvent(OnUseAnimationEvent,           Used);


        private void ForwardAsEvent(Action animatorEvent, Action customEvent)
        {
            if (_logEvents)
            {
                Debug.Log($"[Frame:{Time.frameCount - 1}] " +
                          $"Received {animatorEvent.Method.Name}, forwarding as {customEvent.Method.Name}");
            }
            customEvent?.Invoke();
        }
    }
}

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
        [SerializeField] private Animator penguinAnimator;
        [SerializeField] private bool logEvents = false;

        /*
        Reminder: These parameters _must_ match the names in mecanim.
        Unfortunately there is no easy way to generate the parameter names,
        so just be careful to make sure that they match the parameters listed in the Unity Animator.
        */
        [SerializeField] private readonly string paramXMotionValue = "XMotionIntensity";
        [SerializeField] private readonly string paramYMotionValue = "YMotionIntensity";
        [SerializeField] private readonly string paramIsGrounded   = "IsGrounded";
        [SerializeField] private readonly string paramIsUpright    = "IsUpright";
        [SerializeField] private readonly string paramLie          = "LieDown";
        [SerializeField] private readonly string paramStand        = "StandUp";
        [SerializeField] private readonly string paramJump         = "JumpUp";
        [SerializeField] private readonly string paramFire         = "Fire";
        [SerializeField] private readonly string paramUse          = "Use";

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
            penguinAnimator.ResetTrigger(paramLie);
            penguinAnimator.ResetTrigger(paramStand);
            penguinAnimator.ResetTrigger(paramJump);
            penguinAnimator.ResetTrigger(paramFire);
            penguinAnimator.ResetTrigger(paramUse);
        }

        public Vector2 SkeletalRootPosition => penguinAnimator.rootPosition;

        public void SetParamXMotionIntensity(float ratio) => penguinAnimator.SetFloat(paramXMotionValue, ratio);
        public void SetParamYMotionIntensity(float ratio) => penguinAnimator.SetFloat(paramYMotionValue, ratio);
        
        public void SetParamIsGrounded(bool value)        => penguinAnimator.SetBool(paramIsGrounded, value);
        public void SetParamIsUpright(bool value)         => penguinAnimator.SetBool(paramIsUpright, value);
        
        public void TriggerParamLieDownParameter()        => penguinAnimator.SetTrigger(paramLie);
        public void TriggerParamStandUpParameter()        => penguinAnimator.SetTrigger(paramStand);
        public void TriggerParamJumpUpParameter()         => penguinAnimator.SetTrigger(paramJump);
        public void TriggerParamFireParameter()           => penguinAnimator.SetTrigger(paramFire);
        public void TriggerParamUseParameter()            => penguinAnimator.SetTrigger(paramUse);
        
        
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
            if (logEvents)
            {
                Debug.Log($"[Frame:{Time.frameCount - 1}] " +
                          $"Received {animatorEvent.Method.Name}, forwarding as {customEvent.Method.Name}");
            }
            customEvent?.Invoke();
        }
    }
}

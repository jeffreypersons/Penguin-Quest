using System;
using UnityEngine;


namespace PenguinQuest.Controllers
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
    [System.Serializable]
    [RequireComponent(typeof(Animator))]
    public class PenguinAnimation : MonoBehaviour
    {
        [SerializeField] Animator penguinAnimator;
        [SerializeField] private bool logEvents = false;

        [SerializeField] private readonly string paramXMotionValue = "XMotionIntensity";
        [SerializeField] private readonly string paramYMotionValue = "YMotionIntensity";
        [SerializeField] private readonly string paramIsGrounded   = "IsGrounded";
        [SerializeField] private readonly string paramIsUpright    = "IsUpright";
        [SerializeField] private readonly string paramLie          = "LieDown";
        [SerializeField] private readonly string paramStand        = "StandUp";
        [SerializeField] private readonly string paramJump         = "JumpUp";
        [SerializeField] private readonly string paramFire         = "Fire";
        [SerializeField] private readonly string paramUse          = "Use";

        public event Action OnJumpImpulse  = default;
        public event Action OnLieDownStart = default;
        public event Action OnLieDownMid   = default;
        public event Action OnLieDownEnd   = default;
        public event Action OnStandUpStart = default;
        public event Action OnStandUpEnd   = default;
        public event Action OnFire         = default;
        public event Action OnUse          = default;


        public void ResetAllTriggers()
        {
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
        


        private void OnJumpUpAnimationEventImpulse()  => ForwardAsEvent(OnJumpUpAnimationEventImpulse,  OnJumpImpulse);

        private void OnLieDownAnimationEventStart() => ForwardAsEvent(OnLieDownAnimationEventStart, OnLieDownStart);
        private void OnLieDownAnimationEventMid()   => ForwardAsEvent(OnLieDownAnimationEventMid,   OnLieDownMid);
        private void OnLiedownAnimationEventEnd()   => ForwardAsEvent(OnLiedownAnimationEventEnd,   OnLieDownEnd);

        private void OnStandUpAnimationEventStart() => ForwardAsEvent(OnStandUpAnimationEventStart, OnStandUpStart);
        private void OnStandUpAnimationEventEnd()   => ForwardAsEvent(OnStandUpAnimationEventEnd,   OnStandUpEnd);

        private void OnFireAnimationEvent()         => ForwardAsEvent(OnFireAnimationEvent,         OnFire);
        private void OnUseAnimationEvent()          => ForwardAsEvent(OnUseAnimationEvent,          OnUse);


        private void ForwardAsEvent(Action animatorEvent, Action customEvent)
        {
            if (logEvents)
            {
                Debug.Log(
                    $"PenguinAnimationEventReciever: " +
                    $"[frame:{Time.frameCount - 1}]: " +
                    $"ForwardAsEvent: " +
                    $"Received {animatorEvent.Method.Name}, forwarding as {customEvent.Method.Name}");
            }
            customEvent?.Invoke();
        }
    }
}

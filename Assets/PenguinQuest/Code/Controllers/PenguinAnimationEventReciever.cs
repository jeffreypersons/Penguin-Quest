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
    //[RequireComponent(typeof(Animator))]
    public class PenguinAnimationEventReciever : MonoBehaviour
    {
        [SerializeField] private bool logEvents = false;

        public event Action OnJumpImpulse  = default;
        public event Action OnLiedownStart = default;
        public event Action OnLiedownMid   = default;
        public event Action OnLiedownEnd   = default;
        public event Action OnStandupStart = default;
        public event Action OnStandupEnd   = default;
        public event Action OnFire         = default;
        public event Action OnUse          = default;


        private void OnJumpAnimationEventImpulse()  => ForwardAsEvent(OnJumpAnimationEventImpulse,  OnJumpImpulse);

        private void OnLiedownAnimationEventStart() => ForwardAsEvent(OnLiedownAnimationEventStart, OnLiedownStart);
        private void OnLiedownAnimationEventMid()   => ForwardAsEvent(OnLiedownAnimationEventMid,   OnLiedownMid);
        private void OnLiedownAnimationEventEnd()   => ForwardAsEvent(OnLiedownAnimationEventEnd,   OnLiedownEnd);

        private void OnStandupAnimationEventStart() => ForwardAsEvent(OnStandupAnimationEventStart, OnStandupStart);
        private void OnStandupAnimationEventEnd()   => ForwardAsEvent(OnStandupAnimationEventEnd,   OnStandupEnd);

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

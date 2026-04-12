using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateStandingUp : FsmState<PenguinStateId, PenguinEntity>
    {
        private const float FallbackTimeoutSeconds = 2.0f;
        private float _elapsedTime;

        public PenguinStateStandingUp() : base() { }

        protected override void OnInitialize()
        {
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.StandUpStarted), HandleStandUpAnimationStarted);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.StandUpEnded),   HandleStandUpAnimationFinished);
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.StandUp);
            _elapsedTime = 0f;
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            // todo: handle momentum during stand up and 'sliding' bounding box adjustments
            _elapsedTime += Time.fixedDeltaTime;
            if (_elapsedTime > FallbackTimeoutSeconds)
            {
                Debug.LogWarning("StandingUp animation did not complete in time - forcing transition to Feet");
                base.SignalMoveToNextState(PenguinStateId.Feet);
            }
        }

        private void HandleStandUpAnimationStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            Blob.Skeleton.ColliderConstraints = PenguinColliderConstraints.DisableOuter;
        }

        private void HandleStandUpAnimationFinished()
        {
            // enable all colliders as we are now fully onFeet
            Blob.Skeleton.ColliderConstraints = PenguinColliderConstraints.None;

            base.SignalMoveToNextState(PenguinStateId.Feet);
        }
    }
}

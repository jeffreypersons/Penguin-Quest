using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateLyingDown : FsmState<PenguinStateId, PenguinEntity>
    {
        private const float FallbackTimeoutSeconds = 2.0f;
        private float _elapsedTime;

        public PenguinStateLyingDown() : base() { }

        protected override void OnInitialize()
        {
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.LieDownStarted),  HandleLieDownAnimationStarted);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.LieDownMidpoint), HandleLieDownAnimationMidpoint);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.LieDownEnded),    HandleLieDownAnimationFinished);
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.LieDown);
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
                Debug.LogWarning("LyingDown animation did not complete in time - forcing transition to Belly");
                base.SignalMoveToNextState(PenguinStateId.Belly);
            }
        }

        private void HandleLieDownAnimationStarted()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            Blob.Skeleton.ColliderConstraints =
                PenguinColliderConstraints.DisableFeet;
        }

        private void HandleLieDownAnimationMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            Blob.Skeleton.ColliderConstraints =
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;
        }

        private void HandleLieDownAnimationFinished()
        {
            base.SignalMoveToNextState(PenguinStateId.Belly);
        }
    }
}

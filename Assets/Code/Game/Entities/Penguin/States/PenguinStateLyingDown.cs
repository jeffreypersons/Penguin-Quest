using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateLyingDown : FsmState<PenguinStateId, PenguinEntity>
    {
        public PenguinStateLyingDown() : base() { }

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.LieDownStarted),  HandleLieDownAnimationStarted);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.LieDownMidpoint), HandleLieDownAnimationMidpoint);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.LieDownEnded),    HandleLieDownAnimationFinished);
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.LieDown);
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            // todo: handle momentum during stand up and 'sliding' bounding box adjustments
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

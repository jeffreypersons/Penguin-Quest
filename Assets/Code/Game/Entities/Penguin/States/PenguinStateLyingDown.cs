using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateLyingDown : FsmState<PenguinStateId, PenguinFsmSharedData>
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
            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            Blob.Skeleton.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;

            Blob.CharacterController.Settings = Blob.OnBellySettings;
            Blob.Skeleton.ReadjustBoundingBox(
                offset:     new Vector2( 0,  5),
                size:       new Vector2(25, 10),
                edgeRadius: 1.25f
            );

            base.SignalMoveToNextState(PenguinStateId.Belly);
        }
    }
}

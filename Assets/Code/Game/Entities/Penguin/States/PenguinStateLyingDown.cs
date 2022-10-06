using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateLyingDown : FsmState<PenguinStateId, PenguinBlob>
    {
        public PenguinStateLyingDown() : base() { }

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.Animation.LieDownStarted,  HandleLieDownAnimationStarted);
            RegisterEvent(Blob.Animation.LieDownMidpoint, HandleLieDownAnimationMidpoint);
            RegisterEvent(Blob.Animation.LieDownEnded,    HandleLieDownAnimationFinished);
        }

        protected override void OnEnter()
        {
            Blob.Animation.ResetAllTriggers();
            Blob.Animation.TriggerParamLieDownParameter();
        }

        protected override void OnExit()
        {
            Blob.Animation.ResetAllTriggers();
        }


        private void HandleLieDownAnimationStarted()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            Blob.ColliderConstraints =
                PenguinColliderConstraints.DisableFeet;
        }

        private void HandleLieDownAnimationMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            Blob.ColliderConstraints =
                PenguinColliderConstraints.DisableFeet  |
                PenguinColliderConstraints.DisableFlippers;
        }

        private void HandleLieDownAnimationFinished()
        {
            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            Blob.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;

            Blob.CharacterController.Settings = Blob.OnBellySettings;
            Blob.ReadjustBoundingBox(
                offset:     new Vector2( 0,  5),
                size:       new Vector2(25, 10),
                edgeRadius: 1.25f
            );

            base.SignalMoveToNextState(PenguinStateId.Belly);
        }
    }
}

using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateStandingUp : FsmState<PenguinStateId, PenguinBlob>
    {
        public PenguinStateStandingUp() : base() { }

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimation.EventId.StandUpStarted), HandleStandUpAnimationStarted);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimation.EventId.StandUpEnded),   HandleStandUpAnimationFinished);
        }

        protected override void OnEnter()
        {
            Blob.Animation.ResetAllTriggers();
            Blob.Animation.TriggerParamStandUpParameter();
        }

        protected override void OnExit()
        {
            Blob.Animation.ResetAllTriggers();
        }


        private void HandleStandUpAnimationStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            Blob.ColliderConstraints = PenguinColliderConstraints.DisableOuter;
        }

        private void HandleStandUpAnimationFinished()
        {
            // enable all colliders as we are now fully onFeet
            Blob.ColliderConstraints = PenguinColliderConstraints.None;

            Blob.CharacterController.Settings = Blob.OnFeetSettings;
            Blob.ReadjustBoundingBox(
                offset: new Vector2(-0.3983436f, 14.60247f),
                size:   new Vector2(13.17636f,   28.28143f),
                edgeRadius: 0.68f
            );

            base.SignalMoveToNextState(PenguinStateId.Feet);

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

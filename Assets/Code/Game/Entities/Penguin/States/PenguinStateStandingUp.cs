using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateStandingUp : FsmState<PenguinStateId, PenguinEntity>
    {
        public PenguinStateStandingUp() : base() { }

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.StandUpStarted), HandleStandUpAnimationStarted);
            RegisterEvent(Blob.Animation.LookupEvent(PenguinAnimationEventId.StandUpEnded),   HandleStandUpAnimationFinished);
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.StandUp);
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            // todo: handle momentum during stand up and 'sliding' bounding box adjustments
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

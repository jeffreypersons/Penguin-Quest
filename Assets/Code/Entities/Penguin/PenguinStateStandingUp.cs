using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    public class PenguinStateStandingUp : FsmState
    {
        private PenguinFsmDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateStandingUp(string name, PenguinFsmDriver driver,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        protected override void OnIntialize()
        {
            RegisterEvent(_blob.Animation.StandUpStarted, HandleStandUpAnimationStarted);
            RegisterEvent(_blob.Animation.StandUpEnded,   HandleStandUpAnimationFinished);
        }

        protected override void OnEnter()
        {
            _blob.Animation.ResetAllTriggers();
            _blob.Animation.TriggerParamStandUpParameter();
        }

        protected override void OnExit()
        {
            _blob.Animation.ResetAllTriggers();
        }


        private void HandleStandUpAnimationStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            _blob.ColliderConstraints = PenguinColliderConstraints.DisableOuter;
        }

        private void HandleStandUpAnimationFinished()
        {
            // enable all colliders as we are now fully onFeet
            _blob.ColliderConstraints = PenguinColliderConstraints.None;

            _blob.CharacterController.Settings = _blob.OnFeetSettings;
            _blob.ReadjustBoundingBox(
                offset: new Vector2(-0.3983436f, 14.60247f),
                size:   new Vector2(13.17636f,   28.28143f),
                edgeRadius: 0.68f
            );

            _driver.MoveToState(PenguinBlob.StateIdFeet);

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

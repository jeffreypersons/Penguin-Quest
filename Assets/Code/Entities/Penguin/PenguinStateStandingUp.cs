using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateStandingUp : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateStandingUp(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name, eventRegistry: null)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        public override void OnEnter()
        {
            _blob.Animation.StandUpStarted.AddListener(OnStandUpAnimationStarted);
            _blob.Animation.StandUpEnded  .AddListener(OnStandUpAnimationFinished);

            _blob.Animation.TriggerParamStandUpParameter();
        }

        public override void OnExit()
        {
            _blob.Animation.StandUpStarted.RemoveListener(OnStandUpAnimationStarted);
            _blob.Animation.StandUpEnded.RemoveListener(OnStandUpAnimationFinished);
        }


        private void OnStandUpAnimationStarted(string _)
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            _blob.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        private void OnStandUpAnimationFinished(string _)
        {
            // enable all colliders as we are now fully onFeet
            _blob.ColliderConstraints = PenguinColliderConstraints.None;

            _blob.CharacterController.Settings = _blob.OnFeetSettings;
            _blob.ReadjustBoundingBox(
                offset: new Vector2(-0.3983436f, 14.60247f),
                size:   new Vector2(13.17636f,   28.28143f),
                edgeRadius: 0.68f
            );

            _driver.MoveToState(_driver.StateFeet);

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

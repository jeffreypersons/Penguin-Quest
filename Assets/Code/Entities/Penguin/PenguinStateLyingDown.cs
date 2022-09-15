using UnityEngine;
using PQ.Common.States;


namespace PQ.Entities.Penguin
{
    public class PenguinStateLyingDown : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateLyingDown(string name, PenguinStateMachineDriver driver,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        protected override void OnIntialize()
        {
            RegisterEvent(_blob.Animation.LieDownStarted,  HandleLieDownAnimationStarted);
            RegisterEvent(_blob.Animation.LieDownMidpoint, HandleLieDownAnimationMidpoint);
            RegisterEvent(_blob.Animation.LieDownEnded,    HandleLieDownAnimationFinished);
        }

        protected override void OnEnter()
        {
            _blob.Animation.ResetAllTriggers();
            _blob.Animation.TriggerParamLieDownParameter();
        }

        protected override void OnExit()
        {
            _blob.Animation.ResetAllTriggers();
        }


        private void HandleLieDownAnimationStarted()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _blob.ColliderConstraints =
                PenguinColliderConstraints.DisableFeet;
        }

        private void HandleLieDownAnimationMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _blob.ColliderConstraints =
                PenguinColliderConstraints.DisableFeet  |
                PenguinColliderConstraints.DisableFlippers;
        }

        private void HandleLieDownAnimationFinished()
        {
            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            _blob.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;

            _blob.CharacterController.Settings = _blob.OnBellySettings;
            _blob.ReadjustBoundingBox(
                offset:     new Vector2( 0,  5),
                size:       new Vector2(25, 10),
                edgeRadius: 1.25f
            );

            _driver.MoveToState(_driver.StateBelly);
        }
    }
}

﻿using UnityEngine;
using PQ.Common;


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
            //RegisterEvent(_blob.Animation.LieDownStarted, HandleLieDownAnimationStarted);
        }

        protected override void OnEnter()
        {
            _blob.Animation.LieDownStarted .AddListener(HandleLieDownAnimationStarted);
            _blob.Animation.LieDownMidpoint.AddListener(HandleLieDownAnimationMidpoint);
            _blob.Animation.LieDownEnded   .AddListener(HandleLieDownAnimationFinished);

            _blob.Animation.TriggerParamLieDownParameter();
        }

        protected override void OnExit()
        {
            _blob.Animation.LieDownStarted .RemoveListener(HandleLieDownAnimationStarted);
            _blob.Animation.LieDownMidpoint.RemoveListener(HandleLieDownAnimationMidpoint);
            _blob.Animation.LieDownEnded   .RemoveListener(HandleLieDownAnimationFinished);
        }


        private void HandleLieDownAnimationStarted(IEventPayload.Empty _)
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _blob.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        private void HandleLieDownAnimationMidpoint(IEventPayload.Empty _)
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _blob.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet        |
                PenguinColliderConstraints.DisableFlippers;
        }

        private void HandleLieDownAnimationFinished(IEventPayload.Empty _)
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

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
            //
            // todo: configure bounding box for onbelly mode, and enable the collider back here,
            //       after disabling in animation start, and then update in the following way...
            //       penguinBlob.ColliderBoundingBox.bounds such that offset(x=0, y=5), size(x=25, y=10), edge-radius(1.25)
            //
        }
    }
}

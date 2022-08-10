using System;
using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateOnBelly(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }


        public override void Enter()
        {
            _eventCenter.standUpCommand.AddListener(OnStandUpInputReceived);
            _blob.CharacterController.GroundContactChanged += OnGroundContactChanged;

            _blob.CharacterController.Settings = _blob.OnBellySettings;
        }

        public override void Exit()
        {
            _eventCenter.standUpCommand.RemoveListener(OnStandUpInputReceived);
            _blob.CharacterController.GroundContactChanged -= OnGroundContactChanged;
        }


        // todo: look into putting the ground check animation update somewhere else more reusable, like a penguin base state
        private void OnGroundContactChanged(bool isGrounded) => _blob.Animation.SetParamIsGrounded(isGrounded);
        private void OnStandUpInputReceived(string _) => _driver.MoveToState(_driver.StateStandingUp);
    }
}

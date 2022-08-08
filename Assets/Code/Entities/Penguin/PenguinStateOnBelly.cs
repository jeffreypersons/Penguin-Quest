using System;
using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        public PenguinStateOnBelly(PenguinStateMachineDriver driver, string name, PenguinBlob blob) : base(name)
        {
            _blob = blob;
            _driver = driver;
        }

        
        public override void Enter()
        {
            _blob.CharacterController.Settings = _blob.OnBellySettings;

            GameEventCenter.standUpCommand.AddListener(OnStandUpInputReceived);
        }

        public override void Exit()
        {
            GameEventCenter.standUpCommand.RemoveListener(OnStandUpInputReceived);
        }

        private void OnStandUpInputReceived(string _) => _driver.MoveToState(_driver.StateStandingUp);
    }
}

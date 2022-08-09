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
            _blob.CharacterController.Settings = _blob.OnBellySettings;

            _eventCenter.standUpCommand.AddListener(OnStandUpInputReceived);
        }

        public override void Exit()
        {
            _eventCenter.standUpCommand.RemoveListener(OnStandUpInputReceived);
        }

        private void OnStandUpInputReceived(string _) => _driver.MoveToState(_driver.StateStandingUp);
    }
}

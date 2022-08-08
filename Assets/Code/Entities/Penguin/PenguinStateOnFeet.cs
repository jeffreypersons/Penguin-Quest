using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        public PenguinStateOnFeet(PenguinStateMachineDriver driver, string name, PenguinBlob blob) : base(name)
        {
            _blob = blob;
            _driver = driver;
        }

        public override void Enter()
        {
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);

            _blob.CharacterController.Settings = _blob.OnFeetSettings;
        }

        public override void Exit()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
        }

        private void OnLieDownInputReceived(string _) => _driver.MoveToState(_driver.StateLyingDown);
    }
}

using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateOnFeet(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        public override void Enter()
        {
            _eventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);

            _blob.CharacterController.Settings = _blob.OnFeetSettings;
        }

        public override void Exit()
        {
            _eventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
        }

        private void OnLieDownInputReceived(string _) => _driver.MoveToState(_driver.StateLyingDown);
    }
}

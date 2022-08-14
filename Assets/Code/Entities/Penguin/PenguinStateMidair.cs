using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateMidair(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }


        public override void Enter()
        {
            _blob.CharacterController.GroundContactChanged += OnGroundContactChanged;

            _blob.Animation.TriggerParamJumpUpParameter();
        }

        public override void Exit()
        {
            _blob.CharacterController.GroundContactChanged -= OnGroundContactChanged;

            // reset any triggers such that any pending animation events are cleared out to avoid them
            // from firing automatically on landing
            _blob.Animation.ResetAllTriggers();
        }

        private void OnGroundContactChanged(bool isGrounded)
        {
            _blob.Animation.SetParamIsGrounded(isGrounded);
            if (isGrounded)
            {
                _driver.MoveToState(_driver.PreviousState);
            }
        }
    }
}

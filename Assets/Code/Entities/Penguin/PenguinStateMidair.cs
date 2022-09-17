using UnityEngine;
using PQ.Common.States;


namespace PQ.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState
    {
        private PenguinFsmDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateMidair(string name, PenguinFsmDriver driver,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        protected override void OnIntialize()
        {
            RegisterEvent(_blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged);
        }

        protected override void OnEnter()
        {
            _blob.Animation.TriggerParamJumpUpParameter();
        }

        protected override void OnExit()
        {
            // reset any triggers such that any pending animation events are cleared out to avoid them
            // from firing automatically on landing
            _blob.Animation.ResetAllTriggers();
        }


        private void HandleGroundContactChanged(bool isGrounded)
        {
            _blob.Animation.SetParamIsGrounded(isGrounded);
            if (isGrounded)
            {
                _driver.MoveToState(_driver.PreviousState);
            }
        }
    }
}

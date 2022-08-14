using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateJump : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateJump(PenguinStateMachineDriver driver, string name,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }


        public override void Enter()
        {
            _blob.Animation.JumpLiftOff += ApplyJumpImpulse;

            _blob.Animation.TriggerParamJumpUpParameter();
        }

        public override void Exit()
        {
            _blob.Animation.JumpLiftOff -= ApplyJumpImpulse;
        }

        void OnJumpInputReceived(string _)
        {
            _blob.Animation.TriggerParamJumpUpParameter();
        }

        void ApplyJumpImpulse()
        {
            _blob.CharacterController.Jump();
        }
    }
}

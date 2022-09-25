using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState<PenguinStateId, PenguinBlob>
    {
        public PenguinStateMidair() : base() { }

        protected override void OnIntialize()
        {
            RegisterEvent(Blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged);
        }

        protected override void OnEnter()
        {
            Blob.Animation.TriggerParamJumpUpParameter();
        }

        protected override void OnExit()
        {
            // reset any triggers such that any pending animation events are cleared out to avoid them
            // from firing automatically on landing
            Blob.Animation.ResetAllTriggers();
        }


        private void HandleGroundContactChanged(bool isGrounded)
        {
            Blob.Animation.SetParamIsGrounded(isGrounded);
            if (isGrounded)
            {
                base.SignalMoveToLastState();
            }
        }
    }
}

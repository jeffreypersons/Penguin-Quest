using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState<PenguinStateId, PenguinBlob>
    {
        public PenguinStateMidair() : base() { }

        protected override void OnIntialize()
        {
            //RegisterEvent(Blob.CharacterController.OnGroundContactChanged, HandleGroundContactChanged);
        }

        protected override void OnEnter()
        {
            Blob.Animation.AddTriggerToQueue(PenguinAnimationParamId.JumpUp);
        }

        protected override void OnExit()
        {
            // no op
        }


        private void HandleGroundContactChanged(bool isGrounded)
        {
            Blob.Animation.SetBool(PenguinAnimationParamId.IsGrounded, isGrounded);
            if (isGrounded)
            {
                base.SignalMoveToPreviousState();
            }
        }
    }
}

using UnityEngine;
using PQ.Common.Fsm;
using PQ.Common.Physics;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinStateId, PenguinEntity>
    {
        private bool _grounded;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnBelly() : base() { }


        protected override void OnIntialize()
        {
            RegisterEvent(Blob.EventBus.standUpCommand,      HandleStandUpInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
            RegisterEvent(Blob.Config.OnChanged,             HandleConfigChanged);
        }

        protected override void OnEnter()
        {
            // turn off feet and flippers, since they overlap when sliding around
            _horizontalInput = new(HorizontalInput.Type.None);
            Blob.Skeleton.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;
            HandleConfigChanged();
        }

        protected override void OnExit()
        {
            // no op
        }

        protected override void OnFixedUpdate()
        {
            if (!Mathf.Approximately(_horizontalInput.value, 0f))
            {
                Blob.PhysicsBody.Flip(horizontal: _horizontalInput.value < 0, vertical: false);
            }

            // todo: check inputAxis.y for jumps

            Vector2 velocity = new(
                x: Blob.Config.maxHorizontalSpeedUpright * _horizontalInput.value,
                y: _grounded ? 0 : Blob.PhysicsBody.Gravity
            );

            Blob.PhysicsBody.Move(velocity * Time.fixedDeltaTime);
            _grounded = Blob.PhysicsBody.IsContacting(CollisionFlags2D.Below);
        }

        protected override void OnUpdate()
        {
            // no op
        }
        
        private void HandleConfigChanged()
        {
            // todo: after setting bounds, do overlap resolution before ground check
            // todo: look into putting all this into the physics body class, as it's something we want to do nearly everytime bounds are changed
            Blob.PhysicsBody.SetAABBMinMax(Blob.Config.boundsMinUpright, Blob.Config.boundsMaxUpright, Blob.Config.skinWidthUpright);
            _grounded = Blob.PhysicsBody.IsContacting(CollisionFlags2D.Below);
        }

        private void HandleStandUpInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.StandingUp);
        }

        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
        }
    }
}

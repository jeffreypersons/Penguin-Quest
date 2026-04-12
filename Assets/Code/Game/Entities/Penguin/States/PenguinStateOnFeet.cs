using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState<PenguinStateId, PenguinEntity>
    {
        private bool _jumpRequested;
        private HorizontalInput _horizontalInput;

        public PenguinStateOnFeet() : base() { }

        protected override void OnInitialize()
        {
            RegisterEvent(Blob.EventBus.jumpCommand,         HandleJumpInputReceived);
            RegisterEvent(Blob.EventBus.lieDownCommand,      HandleLieDownInputReceived);
            RegisterEvent(Blob.EventBus.movementInputChange, HandleMoveHorizontalChanged);
            RegisterEvent(Blob.Config.OnChangedInEditor,     HandleConfigChanged);
        }

        protected override void OnEnter()
        {
            // no need to turn off feet and flippers, since they overlap when sliding around
            _jumpRequested = false;
            _horizontalInput = new(HorizontalInput.Type.None);
            Blob.Skeleton.ColliderConstraints = PenguinColliderConstraints.None;
            HandleConfigChanged();
        }

        protected override void OnExit()
        {
            _jumpRequested = false;
            _horizontalInput = new(HorizontalInput.Type.None);
        }

        protected override void OnFixedUpdate()
        {
            if (!Mathf.Approximately(_horizontalInput.value, 0f))
            {
                Blob.PhysicsBody.Flip(horizontal: _horizontalInput.value < 0, vertical: false);
            }

            float verticalVelocity = Blob.IsGrounded ? 0f : Blob.PhysicsBody.Gravity;
            if (_jumpRequested && Blob.IsGrounded)
            {
                verticalVelocity = Blob.Config.jumpImpulse;
                _jumpRequested = false;
            }

            Vector2 velocity = new(
                x: Blob.Config.maxHorizontalSpeedUpright * _horizontalInput.value,
                y: verticalVelocity
            );

            Blob.PhysicsBody.Move(velocity * Time.fixedDeltaTime);

            if (verticalVelocity > 0f)
            {
                base.SignalMoveToNextState(PenguinStateId.Midair);
            }
        }

        protected override void OnUpdate()
        {

        }


        private void HandleConfigChanged()
        {
            Blob.PhysicsBody.SetAABBMinMax(Blob.Config.boundsMinUpright, Blob.Config.boundsMaxUpright, Blob.Config.skinWidthUpright);
        }

        private void HandleJumpInputReceived()
        {
            _jumpRequested = true;
        }

        private void HandleLieDownInputReceived()
        {
            base.SignalMoveToNextState(PenguinStateId.LyingDown);
        }

        private void HandleMoveHorizontalChanged(HorizontalInput state)
        {
            _horizontalInput = state;
        }
    }
}

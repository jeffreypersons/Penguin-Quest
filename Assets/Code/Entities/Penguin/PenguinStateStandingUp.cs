using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateStandingUp : FsmState<PenguinFsmParams>
    {
        private PenguinBlob _blob;
        private PenguinFsmParams _fsmParams;
        public PenguinStateStandingUp(string name, PenguinBlob blob, ref PenguinFsmParams fsmParams)
            : base(name) { _blob = blob; _fsmParams = fsmParams; }

        public override void Enter()
        {
            _blob.Animation.LieDownStarted  += OnStandUpAnimationStarted;
            _blob.Animation.LieDownMidpoint += OnStandUpAnimationFinished;

            _blob.Animation.TriggerParamStandUpParameter();
        }

        public override void Exit()
        {
            _blob.Animation.LieDownStarted  -= OnStandUpAnimationStarted;
            _blob.Animation.LieDownMidpoint -= OnStandUpAnimationFinished;
        }


        private void OnStandUpAnimationStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            _blob.Animation.SetParamIsGrounded(true);
            _blob.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        private void OnStandUpAnimationFinished()
        {
            // enable all colliders as we are now fully onFeet
            _blob.ColliderConstraints = PenguinColliderConstraints.None;

            _blob.CharacterController.Settings = _blob.OnFeetSettings;
            _blob.ReadjustBoundingBox(
                offset: new Vector2(-0.3983436f, 14.60247f),
                size:   new Vector2(13.17636f,   28.28143f),
                edgeRadius: 0.68f
            );

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

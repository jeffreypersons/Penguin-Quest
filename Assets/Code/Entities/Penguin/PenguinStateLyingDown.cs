using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateLyingDown : FsmState
    {
        private PenguinBlob _blob;
        public PenguinStateLyingDown(string name, PenguinBlob blob) : base(name) { _blob = blob; }


        public override void Enter()
        {
            _blob.Animation.LieDownStarted  += OnLieDownAnimationStarted;
            _blob.Animation.LieDownMidpoint += OnLieDownAnimationMidpoint;
            _blob.Animation.LieDownEnded    += OnLieDownAnimationFinished;

            _blob.Animation.TriggerParamLieDownParameter();
        }

        public override void Exit()
        {
            _blob.Animation.LieDownStarted  -= OnLieDownAnimationStarted;
            _blob.Animation.LieDownMidpoint -= OnLieDownAnimationMidpoint;
            _blob.Animation.LieDownEnded    -= OnLieDownAnimationFinished;
        }


        private void OnLieDownAnimationStarted()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _blob.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        private void OnLieDownAnimationMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _blob.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet        |
                PenguinColliderConstraints.DisableFlippers;
        }

        private void OnLieDownAnimationFinished()
        {
            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            _blob.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;

            _blob.CharacterController.Settings = _blob.OnBellySettings;
            _blob.ReadjustBoundingBox(
                offset:     new Vector2( 0,  5),
                size:       new Vector2(25, 10),
                edgeRadius: 1.25f
            );


            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
            //
            // todo: configure bounding box for onbelly mode, and enable the collider back here,
            //       after disabling in animation start, and then update in the following way...
            //       penguinBlob.ColliderBoundingBox.bounds such that offset(x=0, y=5), size(x=25, y=10), edge-radius(1.25)
            //
        }
    }
}

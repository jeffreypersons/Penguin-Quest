using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class LieDownHandler : MonoBehaviour
    {
        private PenguinBlob _penguinBlob;

        void Awake()
        {
            _penguinBlob = transform.GetComponent<PenguinBlob>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);
            _penguinBlob.Animation.LieDownStarted  += OnLieDownStarted;
            _penguinBlob.Animation.LieDownMidpoint += OnLieDownMidpoint;
            _penguinBlob.Animation.LieDownEnded    += OnLieDownFinished;
        }

        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
            _penguinBlob.Animation.LieDownStarted  -= OnLieDownStarted;
            _penguinBlob.Animation.LieDownMidpoint -= OnLieDownMidpoint;
            _penguinBlob.Animation.LieDownEnded    -= OnLieDownFinished;
        }

        
        private void OnLieDownInputReceived(string _)
        {
            _penguinBlob.Animation.TriggerParamLieDownParameter();
        }

        private void OnLieDownStarted()
        {
            _penguinBlob.Animation.SetParamIsUpright(false);

            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _penguinBlob.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        private void OnLieDownMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _penguinBlob.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet        |
                PenguinColliderConstraints.DisableFlippers;
        }

        private void OnLieDownFinished()
        {
            // todo: this stuff needs to go in the state machine
            _penguinBlob.CharacterController.Settings = _penguinBlob.OnBellySettings;

            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            _penguinBlob.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;
            
            _penguinBlob.ReadjustBoundingBox(
                offset:     new Vector2(0, 5),
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

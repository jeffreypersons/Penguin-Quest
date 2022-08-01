using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class LieDownHandler : MonoBehaviour
    {
        // todo: move to state machine for lie down state
        [SerializeField] private CharacterController2DSettings _onBellySettings;

        private PenguinEntity _penguinEntity;
        private CharacterController2D _characterController;

        void Awake()
        {
            _penguinEntity       = transform.GetComponent<PenguinEntity>();
            _characterController = transform.GetComponent<CharacterController2D>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);
            _penguinEntity.Animation.LieDownStarted  += OnLieDownStarted;
            _penguinEntity.Animation.LieDownMidpoint += OnLieDownMidpoint;
            _penguinEntity.Animation.LieDownEnded    += OnLieDownFinished;
        }

        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
            _penguinEntity.Animation.LieDownStarted  -= OnLieDownStarted;
            _penguinEntity.Animation.LieDownMidpoint -= OnLieDownMidpoint;
            _penguinEntity.Animation.LieDownEnded    -= OnLieDownFinished;
        }

        
        void OnLieDownInputReceived(string _)
        {
            _penguinEntity.Animation.TriggerParamLieDownParameter();
        }

        void OnLieDownStarted()
        {
            _penguinEntity.Animation.SetParamIsUpright(false);

            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _penguinEntity.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            _penguinEntity.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet        |
                PenguinColliderConstraints.DisableFlippers;
        }

        void OnLieDownFinished()
        {
            // todo: this stuff needs to go in the state machine
            _characterController.Settings = _onBellySettings;

            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            _penguinEntity.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;
            
            _penguinEntity.ReadjustBoundingBox(
                offset:     new Vector2(0, 5),
                size:       new Vector2(25, 10),
                edgeRadius: 1.25f
            );

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
            //
            // todo: configure bounding box for onbelly mode, and enable the collider back here,
            //       after disabling in animation start, and then update in the following way...
            //       penguinEntity.ColliderBoundingBox.bounds such that offset(x=0, y=5), size(x=25, y=10), edge-radius(1.25)
            //
        }
    }
}

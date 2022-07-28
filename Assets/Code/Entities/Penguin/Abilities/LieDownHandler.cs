using UnityEngine;
using PenguinQuest.Framework;
using PenguinQuest.Entities.Penguin.Components;


namespace PenguinQuest.Entities.Penguin.Abilities
{
    public class LieDownHandler : MonoBehaviour
    {
        private PenguinEntity penguinEntity;
        private CharacterController2D characterController;

        // todo: move to state machine for lie down state
        [SerializeField] private CharacterController2DSettings lieDownStateCharacterSettings;

        void Awake()
        {
            penguinEntity       = transform.GetComponent<PenguinEntity>();
            characterController = transform.GetComponent<CharacterController2D>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);
            penguinEntity.Animation.LieDownStarted  += OnLieDownStarted;
            penguinEntity.Animation.LieDownMidpoint += OnLieDownMidpoint;
            penguinEntity.Animation.LieDownEnded    += OnLieDownFinished;
        }

        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
            penguinEntity.Animation.LieDownStarted  -= OnLieDownStarted;
            penguinEntity.Animation.LieDownMidpoint -= OnLieDownMidpoint;
            penguinEntity.Animation.LieDownEnded    -= OnLieDownFinished;
        }

        
        void OnLieDownInputReceived(string _)
        {
            penguinEntity.Animation.TriggerParamLieDownParameter();
        }

        void OnLieDownStarted()
        {
            penguinEntity.Animation.SetParamIsUpright(false);

            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            penguinEntity.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownMidpoint()
        {
            // disable our box and feet, to prevent catching on edges when changing posture from OnFeet to OnBelly
            penguinEntity.ColliderConstraints =
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet        |
                PenguinColliderConstraints.DisableFlippers;
        }

        void OnLieDownFinished()
        {
            // todo: this stuff needs to go in the state machine
            characterController.Settings = lieDownStateCharacterSettings;

            // keep our feet and flippers disabled to avoid interference with ground while OnBelly,
            // but enable everything else including bounding box
            penguinEntity.ColliderConstraints =
                 PenguinColliderConstraints.DisableFeet |
                 PenguinColliderConstraints.DisableFlippers;
            
            penguinEntity.ReadjustBoundingBox(
                offset:     new Vector2(0, 5),
                size:       new Vector2(25, 10),
                edgeRadius: 1.25f
            );

            // todo: find a good way of having data for sliding and for upright that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
            //
            // todo: configure bounding box for onbelly mode, and enable the collider back here,
            //       after disabling in animation start, and then update in the following way...
            //       penguinEntity.ColliderBoundingBox.bounds such that offset(x=0, y=5), size(x=25, y=10), edge-radius(1.25)
            //
        }
    }
}

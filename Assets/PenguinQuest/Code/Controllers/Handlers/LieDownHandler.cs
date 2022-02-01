using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.AlwaysOnComponents;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(PenguinEntity))]
    [RequireComponent(typeof(CharacterController2D))]
    public class LieDownHandler : MonoBehaviour
    {
        private PenguinEntity         penguinEntity;
        private CharacterController2D characterController;

        void Awake()
        {
            penguinEntity       = transform.GetComponent<PenguinEntity>();
            characterController = transform.GetComponent<CharacterController2D>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInput);
            penguinEntity.Animation.OnLieDownStart += OnLieDownAnimationEventStart;
            penguinEntity.Animation.OnLieDownMid   += OnLieDownAnimationEventMid;
            penguinEntity.Animation.OnLieDownEnd   += OnLieDownAnimationEventEnd;
        }
        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInput);
            penguinEntity.Animation.OnLieDownStart -= OnLieDownAnimationEventStart;
            penguinEntity.Animation.OnLieDownMid   -= OnLieDownAnimationEventMid;
            penguinEntity.Animation.OnLieDownEnd   -= OnLieDownAnimationEventEnd;
        }

        
        void OnLieDownInput(string _)
        {
            penguinEntity.Animation.TriggerParamLieDownParameter();
        }

        void OnLieDownAnimationEventStart()
        {
            penguinEntity.Animation.SetParamIsUpright(false);
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventMid()
        {
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventEnd()
        {
            // todo: this stuff needs to go in the state machine
            characterController.MaintainPerpendicularityToSurface = true;
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;
        }
    }
}

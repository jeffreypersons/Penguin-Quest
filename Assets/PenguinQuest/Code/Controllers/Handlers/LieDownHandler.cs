using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.AlwaysOnComponents;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(PenguinEntity))]
    [RequireComponent(typeof(CharacterController2D))]
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
            characterController.Settings = lieDownStateCharacterSettings;
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;


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

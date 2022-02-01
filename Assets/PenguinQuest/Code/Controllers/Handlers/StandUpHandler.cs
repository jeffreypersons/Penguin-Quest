using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.AlwaysOnComponents;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(PenguinEntity))]
    [RequireComponent(typeof(CharacterController2D))]
    public class StandUpHandler : MonoBehaviour
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
            GameEventCenter.standupCommand.AddListener(OnStandUpInput);
            penguinEntity.Animation.OnStandUpStart += OnStandUpAnimationEventStart;
            penguinEntity.Animation.OnStandUpEnd   += OnStandUpAnimationEventEnd;
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInput);
            penguinEntity.Animation.OnStandUpStart -= OnStandUpAnimationEventStart;
            penguinEntity.Animation.OnStandUpEnd   -= OnStandUpAnimationEventEnd;
        }
        

        void OnStandUpInput(string _)
        {
            penguinEntity.Animation.TriggerParamStandUpParameter();
        }

        void OnStandUpAnimationEventStart()
        {
            penguinEntity.ColliderConstraints = PenguinColliderConstraints.None;

            // enable the feet again, allowing the penguin to 'fall' down to alignment
            penguinEntity.ColliderConstraints &=
                ~PenguinColliderConstraints.DisableFeet;
        }

        void OnStandUpAnimationEventEnd()
        {
            // todo: this stuff needs to go in the state machine
            penguinEntity.Animation.SetParamIsUpright(true);
            characterController.MaintainPerpendicularityToSurface = false;

            // enable the bounding box again, allowing the penguin to 'fall' down to alignment
            penguinEntity.ColliderConstraints &=
                ~PenguinColliderConstraints.DisableBoundingBox;

            // todo: find a good way of having data for sliding and for upright that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

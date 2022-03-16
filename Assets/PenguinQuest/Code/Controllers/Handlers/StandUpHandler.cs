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

        // todo: move to state machine for upright state
        [SerializeField] private CharacterController2DSettings uprightStateCharacterSettings;

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
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            penguinEntity.Animation.SetParamIsGrounded(true);
            penguinEntity.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        void OnStandUpAnimationEventEnd()
        {
            // todo: this stuff needs to go in the state machine
            penguinEntity.Animation.SetParamIsUpright(true);
            characterController.Settings = uprightStateCharacterSettings;

            // enable all colliders as we are now fully upright
            penguinEntity.ColliderConstraints = PenguinColliderConstraints.None;

            // todo: find a good way of having data for sliding and for upright that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(PenguinEntity))]
    public class StandUpHandler : MonoBehaviour
    {
        private PenguinEntity penguinEntity;

        void Awake()
        {
            penguinEntity = gameObject.GetComponent<PenguinEntity>();
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
            transform.GetComponent<GroundHandler>().MaintainPerpendicularityToSurface = false;

            // enable the bounding box again, allowing the penguin to 'fall' down to alignment
            penguinEntity.ColliderConstraints &=
                ~PenguinColliderConstraints.DisableBoundingBox;
        }
    }
}

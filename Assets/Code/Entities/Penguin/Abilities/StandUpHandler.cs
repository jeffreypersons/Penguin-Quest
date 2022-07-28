using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
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
            GameEventCenter.standupCommand.AddListener(OnStandUpInputReceived);
            penguinEntity.Animation.StandUpStarted += OnStandUpStarted;
            penguinEntity.Animation.StandUpEnded   += OnStandUpFinished;
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInputReceived);
            penguinEntity.Animation.StandUpStarted -= OnStandUpStarted;
            penguinEntity.Animation.StandUpEnded   -= OnStandUpFinished;
        }
        

        void OnStandUpInputReceived(string _)
        {
            penguinEntity.Animation.TriggerParamStandUpParameter();
        }

        void OnStandUpStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            penguinEntity.Animation.SetParamIsGrounded(true);
            penguinEntity.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        void OnStandUpFinished()
        {
            // todo: this stuff needs to go in the state machine
            penguinEntity.Animation.SetParamIsUpright(true);
            characterController.Settings = uprightStateCharacterSettings;

            // enable all colliders as we are now fully upright
            penguinEntity.ColliderConstraints = PenguinColliderConstraints.None;

            penguinEntity.ReadjustBoundingBox(
                offset:     new Vector2(-0.3983436f, 14.60247f),
                size:       new Vector2( 13.17636f,  28.28143f),
                edgeRadius: 0.68f
            );

            // todo: find a good way of having data for sliding and for upright that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

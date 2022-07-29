using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class StandUpHandler : MonoBehaviour
    {
        private PenguinBlob           penguinBlob;
        private CharacterController2D characterController;

        // todo: move to state machine for upright state
        [SerializeField] private CharacterController2DSettings uprightStateCharacterSettings;

        void Awake()
        {
            penguinBlob       = transform.GetComponent<PenguinBlob>();
            characterController = transform.GetComponent<CharacterController2D>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.standupCommand.AddListener(OnStandUpInputReceived);
            penguinBlob.Animation.StandUpStarted += OnStandUpStarted;
            penguinBlob.Animation.StandUpEnded   += OnStandUpFinished;
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInputReceived);
            penguinBlob.Animation.StandUpStarted -= OnStandUpStarted;
            penguinBlob.Animation.StandUpEnded   -= OnStandUpFinished;
        }
        

        void OnStandUpInputReceived(string _)
        {
            penguinBlob.Animation.TriggerParamStandUpParameter();
        }

        void OnStandUpStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            penguinBlob.Animation.SetParamIsGrounded(true);
            penguinBlob.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        void OnStandUpFinished()
        {
            // todo: this stuff needs to go in the state machine
            penguinBlob.Animation.SetParamIsUpright(true);
            characterController.Settings = uprightStateCharacterSettings;

            // enable all colliders as we are now fully upright
            penguinBlob.ColliderConstraints = PenguinColliderConstraints.None;

            penguinBlob.ReadjustBoundingBox(
                offset:     new Vector2(-0.3983436f, 14.60247f),
                size:       new Vector2( 13.17636f,  28.28143f),
                edgeRadius: 0.68f
            );

            // todo: find a good way of having data for sliding and for upright that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

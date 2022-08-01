using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class StandUpHandler : MonoBehaviour
    {
        // todo: move to state machine for onFeet state
        [SerializeField] private CharacterController2DSettings _onFeetSettings;

        private PenguinBlob _penguinEntity;
        private CharacterController2D _characterController;

        void Awake()
        {
            _penguinEntity       = transform.GetComponent<PenguinBlob>();
            _characterController = transform.GetComponent<CharacterController2D>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.standupCommand.AddListener(OnStandUpInputReceived);
            _penguinEntity.Animation.StandUpStarted += OnStandUpStarted;
            _penguinEntity.Animation.StandUpEnded   += OnStandUpFinished;
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInputReceived);
            _penguinEntity.Animation.StandUpStarted -= OnStandUpStarted;
            _penguinEntity.Animation.StandUpEnded   -= OnStandUpFinished;
        }
        

        void OnStandUpInputReceived(string _)
        {
            _penguinEntity.Animation.TriggerParamStandUpParameter();
        }

        void OnStandUpStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            _penguinEntity.Animation.SetParamIsGrounded(true);
            _penguinEntity.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        void OnStandUpFinished()
        {
            // todo: this stuff needs to go in the state machine
            _penguinEntity.Animation.SetParamIsUpright(true);
            _characterController.Settings = _onFeetSettings;

            // enable all colliders as we are now fully onFeet
            _penguinEntity.ColliderConstraints = PenguinColliderConstraints.None;

            _penguinEntity.ReadjustBoundingBox(
                offset:     new Vector2(-0.3983436f, 14.60247f),
                size:       new Vector2( 13.17636f,  28.28143f),
                edgeRadius: 0.68f
            );

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

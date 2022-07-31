using UnityEngine;


namespace PQ.Entities.Penguin
{
    public class StandUpHandler : MonoBehaviour
    {
        private PenguinBlob _penguinBlob;

        void Awake()
        {
            _penguinBlob = transform.GetComponent<PenguinBlob>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.standupCommand.AddListener(OnStandUpInputReceived);
            _penguinBlob.Animation.StandUpStarted += OnStandUpStarted;
            _penguinBlob.Animation.StandUpEnded   += OnStandUpFinished;
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInputReceived);
            _penguinBlob.Animation.StandUpStarted -= OnStandUpStarted;
            _penguinBlob.Animation.StandUpEnded   -= OnStandUpFinished;
        }
        

        void OnStandUpInputReceived(string _)
        {
            _penguinBlob.Animation.TriggerParamStandUpParameter();
        }

        void OnStandUpStarted()
        {
            // keep all colliders on _except_ for the bounding box, to prevent catching on edges during posture change
            _penguinBlob.Animation.SetParamIsGrounded(true);
            _penguinBlob.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
        }

        void OnStandUpFinished()
        {
            // todo: this stuff needs to go in the state machine
            _penguinBlob.Animation.SetParamIsUpright(true);
            _penguinBlob.CharacterController.Settings = _penguinBlob.OnFeetSettings;

            // enable all colliders as we are now fully onFeet
            _penguinBlob.ColliderConstraints = PenguinColliderConstraints.None;

            _penguinBlob.ReadjustBoundingBox(
                offset:     new Vector2(-0.3983436f, 14.60247f),
                size:       new Vector2( 13.17636f,  28.28143f),
                edgeRadius: 0.68f
            );

            // todo: find a good way of having data for sliding and for onFeet that can be passed in here,
            //       and those values can be adjusted, perhaps in their own scriptable objects?
        }
    }
}

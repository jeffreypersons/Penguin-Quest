using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    // todo: extract out a generic state controller and put it in FSM as an interface with the abstract state class,
    //       and then use it here, with penguin states and here as a penguin state controller
    public class PenguinStateController : MonoBehaviour
    {
        private PenguinBlob _penguinBlob;
        private CharacterController2D _characterController2D;

        private PenguinOnFeetState  _onFeet;
        private PenguinOnBellyState _onBelly;

        // todo: have this taken care of in the initial state that we enter
        [SerializeField] private CharacterController2DSettings initialStateCharacterSettings;

        private PlayerGameplayInputReceiver input;
        
        private Vector2 initialSpawnPosition;
        private FsmState CurrentState { get; set; }
        private bool IsCurrently(FsmState state)
        {
            return ReferenceEquals(CurrentState, state);
        }

        private bool CanEnterOnFeetState  => !IsCurrently(_onFeet);
        private bool CanEnterOnBellyState => !IsCurrently(_onBelly);


        private void TransitionToState(FsmState newState)
        {
            FsmState oldState = CurrentState;
            Debug.Log($"Transitioning from {oldState} to {newState}");
            oldState.Exit();
            newState.Enter();
            CurrentState = newState;
        }
        
        void Awake()
        {

            _onFeet  = new PenguinOnFeetState ("OnFeet_State");
            _onBelly = new PenguinOnBellyState("OnBelly_State");
            CurrentState = _onFeet;

            _penguinBlob         = gameObject.GetComponent<PenguinBlob>();
            _characterController2D = gameObject.GetComponent<CharacterController2D>();

            // todo: this should be in state machine for onFeet and we should start in a blank state and then
            //       entered rather than assuming we start onFeet here...
            _characterController2D.Settings = initialStateCharacterSettings;

            initialSpawnPosition = _penguinBlob.Rigidbody.position;
            ResetPositioning();
        }

        // todo: this should really be extracted out into a proper spawning system...
        public void ResetPositioning()
        {
            _penguinBlob.Rigidbody.velocity = Vector2.zero;
            _penguinBlob.Rigidbody.position = initialSpawnPosition;
            _penguinBlob.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }

        void Start()
        {

        }


        void Update()
        {
            /***** disable state changes until state subclasses are properly implemented
            CurrentState.Update();

            if (CanEnterOnFeetState)
            {
                TransitionToState(onFeet);
            }
            else if (CanEnterOnBellyState)
            {
                TransitionToState(onBelly);
            }
            */
        }

        void FixedUpdate()
        {
            CurrentState.FixedUpdate();
        }

        void LateUpdate()
        {
            CurrentState.LateUpdate();
        }
    }
}

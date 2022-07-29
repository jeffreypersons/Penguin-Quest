using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    // todo: extract out a generic state controller and put it in FSM as an interface with the abstract state class,
    //       and then use it here, with penguin states and here as a penguin state controller
    public class PenguinStateController : MonoBehaviour
    {
        private PenguinBlob           penguinBlob;
        private CharacterController2D characterController2D;

        private PenguinUprightState upright;
        private PenguinOnBellyState onBelly;

        // todo: have this taken care of in the initial state that we enter
        [SerializeField] private CharacterController2DSettings initialStateCharacterSettings;

        private PlayerInputReceiver input;
        
        private Vector2 initialSpawnPosition;
        private FsmStateBase CurrentState { get; set; }
        private bool IsCurrently(FsmStateBase state)
        {
            return ReferenceEquals(CurrentState, state);
        }

        private bool CanEnterUprightState => !IsCurrently(upright);
        private bool CanEnterOnbellyState => !IsCurrently(onBelly);


        private void TransitionToState(FsmStateBase newState)
        {
            FsmStateBase oldState = CurrentState;
            Debug.Log($"Transitioning from {oldState} to {newState}");
            oldState.Exit();
            newState.Enter();
            CurrentState = newState;
        }
        
        void Awake()
        {

            upright = new PenguinUprightState("Upright_State");
            onBelly = new PenguinOnBellyState("OnBelly_State");
            CurrentState = upright;

            penguinBlob         = gameObject.GetComponent<PenguinBlob>();
            characterController2D = gameObject.GetComponent<CharacterController2D>();

            // todo: this should be in state machine for upright and we should start in a blank state and then
            //       entered rather than assuming we start upright here...
            characterController2D.Settings = initialStateCharacterSettings;

            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();
        }

        // todo: this should really be extracted out into a proper spawning system...
        public void ResetPositioning()
        {
            penguinBlob.Rigidbody.velocity = Vector2.zero;
            penguinBlob.Rigidbody.position = initialSpawnPosition;
            penguinBlob.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }

        void Start()
        {

        }


        void Update()
        {
            /***** disable state changes until state subclasses are properly implemented
            CurrentState.Update();

            if (CanEnterUprightState)
            {
                TransitionToState(upright);
            }
            else if (CanEnterOnbellyState)
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

using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.Fsm;


namespace PenguinQuest.Controllers.Fsm
{
    public class PenguinStateController : MonoBehaviour
    {
        private PenguinUprightState upright;
        private PenguinOnBellyState onBelly;

        private GameplayInputReciever input;
        private FsmState CurrentState { get; set; }
        private bool IsCurrently(FsmState state)
        {
            return ReferenceEquals(CurrentState, state);
        }

        private bool CanEnterUprightState => input.Axes.y > 0.0f && !IsCurrently(upright);
        private bool CanEnterOnbellyState => input.Axes.y < 0.0f && !IsCurrently(onBelly);


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
            upright = new PenguinUprightState("Upright_State");
            onBelly = new PenguinOnBellyState("OnBelly_State");
            CurrentState = upright;
        }

        void Start()
        {

        }
        void Update()
        {
            CurrentState.Update();

            if (CanEnterUprightState)
            {
                TransitionToState(upright);
            }
            else if (CanEnterOnbellyState)
            {
                TransitionToState(onBelly);
            }
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

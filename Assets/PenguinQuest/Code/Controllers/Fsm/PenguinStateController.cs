using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.Fsm;


namespace PenguinQuest.Controllers.Fsm
{
    public class PenguinStateController : MonoBehaviour
    {
        private WalkingState walking;
        private SlidingState sliding;

        private GameplayInputReciever input;
        private FsmState CurrentState { get; set; }
        private bool IsCurrently(FsmState state)
        {
            return ReferenceEquals(CurrentState, state);
        }

        private bool CanEnterUprightState => input.Axes.y > 0.0f && !IsCurrently(walking);
        private bool CanEnterOnbellyState => input.Axes.y < 0.0f && !IsCurrently(sliding);


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
            walking = new WalkingState("Walking_State");
            sliding = new SlidingState("Sliding_State");
            CurrentState = walking;
        }

        void Start()
        {

        }
        void Update()
        {
            CurrentState.Update();

            if (CanEnterUprightState)
            {
                TransitionToState(walking);
            }
            else if (CanEnterOnbellyState)
            {
                TransitionToState(sliding);
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

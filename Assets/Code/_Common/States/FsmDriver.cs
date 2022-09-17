using System;
using UnityEngine;


namespace PQ.Common.States
{
    /*
    Driver for state machine that hooks up state callbacks to MonoBehaviour.

    Note that there are no transitional checks - it's entirely up to the specific implementation to
    determine when and how transitions should occur.
    */
    public abstract class FsmDriver : MonoBehaviour
    {
        private FsmGraph _fsmGraph = null;
        private FsmContext _fsmContext = null;

        public override string ToString() =>
            $"FsmDriver:{{" +
                $"graph:{_fsmGraph}" +
            $"}}";


        /*** External Facing Methods for Driving State Logic ***/

        // Initialization method that MUST be overridden in subclasses; don't forget base.Initialize(initialState)
        protected virtual void InitializeStates(params FsmState[] states)
        {
            _fsmGraph = new FsmGraph(states);
        }

        protected abstract void OnInitialize();

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(FsmState previous, FsmState next) { }


        /*** Internal Hooks to MonoBehavior ***/

        private void Start()
        {
            // since states may have may game object dependencies, we explicitly want to
            // initialize our fsm on start, rather in awake, where those objects may not fully initialized.
            OnInitialize();
            if (_fsmGraph == null)
            {
                throw new InvalidOperationException("States were not initialized - " +
                    "InitializeStates must be called within subclass OnInitialize");
            }

            CurrentState = InitialState;
            CurrentState.Enter();
        }

        private void Update()
        {
            ExecuteTransitionIfPending();
            CurrentState.Update();
        }

        private void FixedUpdate()
        {
            CurrentState.FixedUpdate();
        }

        private void LateUpdate()
        {
            CurrentState.LateUpdate();
        }


        // Update our current state provided that it is distinct from the next
        private bool ExecuteTransitionIfPending()
        {
            if (_nextScheduledState == null)
            {
                return false;
            }
            if (_nextScheduledState == CurrentState)
            {
                throw new ArgumentException(
                    $"Transition from {CurrentState} to {_nextScheduledState} is invalid -" +
                    $" cannot loop to the same state");
            }

            FsmState previous = CurrentState;

            previous.Exit();
            OnTransition(previous, _nextScheduledState);
            _nextScheduledState.Enter();

            PreviousState = previous;
            CurrentState = _nextScheduledState;
            _nextScheduledState = null;
            return true;
        }
    }
}

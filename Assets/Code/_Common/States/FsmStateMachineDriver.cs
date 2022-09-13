using System;
using UnityEngine;


namespace PQ.Common.States
{
    //
    // todo: look into exposing a state machine context that can have a reference given to each state
    //       rather than the current method of passing the entire state machine instance to state instances
    //       so that they can signal MoveToState() more indirectly
    //

    /*
    Driver for state machine that hooks up state callbacks to MonoBehaviour.

    Note that there are no transitional checks - it's entirely up to the specific implementation to
    determine when and how transitions should occur.
    */
    public abstract class FsmStateMachineDriver : MonoBehaviour
    {
        private bool _statesInitialized = false;
        private FsmState _nextScheduledState = null;
        public FsmState InitialState  { get; private set; }
        public FsmState CurrentState  { get; private set; }
        public FsmState PreviousState { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"initialState:{InitialState}," +
                $"currentState:{CurrentState}," +
                $"previousState:{PreviousState}}}";

        // Initialization method that MUST be overridden in subclasses; don't forget base.Initialize(initialState)
        protected virtual void InitializeStates(FsmState initialState, params FsmState[] otherStates)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException($"State 0 received is null");
            }

            initialState.Initialize();
            for (int i = 0; i < otherStates.Length; i++)
            {
                if (InitialState == otherStates[i])
                {
                    throw new ArgumentNullException($"State {i+1} received is null");
                }
                otherStates[i].Initialize();
            }

            InitialState = initialState;
            CurrentState = null;
            PreviousState = null;
            _nextScheduledState = null;
            _statesInitialized = true;
        }

        protected abstract void OnInitialize();

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(FsmState previous, FsmState next) { }

        // Update our current state provided that it is distinct from the next
        public void MoveToState(FsmState next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(
                    $"Transition from {CurrentState} to {next} is invalid -" +
                    $" cannot enter a null state");
            }
            if (next == CurrentState)
            {
                throw new ArgumentException(
                    $"Transition from {CurrentState} to {next} is invalid -" +
                    $" cannot loop to the same state");
            }

            _nextScheduledState = next;
        }

        private bool ExecuteTransitionIfPending()
        {
            if (_nextScheduledState == null || _nextScheduledState == CurrentState)
            {
                return false;
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


        /*** Internal Hooks to MonoBehavior ***/

        private void Start()
        {
            // since states may have may game object dependencies, we explicitly want to
            // initialize our fsm on start, rather in awake, where those objects may not fully initialized.
            OnInitialize();
            if (!_statesInitialized)
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
    }
}

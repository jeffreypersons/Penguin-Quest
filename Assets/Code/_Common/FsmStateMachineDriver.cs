using System;
using UnityEngine;


namespace PQ.Common
{
    /*
    Driver for state machine that hooks up state callbacks to MonoBehaviour.

    Note that there are no transitional checks - it's entirely up to the child class implementation to
    determine when transitions should occur.
    */
    public abstract class FsmStateMachineDriver : MonoBehaviour
    {
        private FsmState _nextScheduledState;

        protected FsmState InitialState { get; private set; }
        protected FsmState CurrentState { get; private set; }

        // Initialization method that MUST be overridden in subclasses; don't forget base.Initialize(initialState)
        protected virtual void Initialize(FsmState initialState)
        {
            InitialState = initialState;
            _nextScheduledState = null;
        }

        // todo: replace with event integration, and/or per frame update with priorities to ensure correct ordering
        // Required method implementation, where transitions are checked and called
        //protected abstract void ExecuteAnyTransitions();

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(FsmState previous, FsmState next) { }

        // Is this our current state in the FSM?
        protected bool IsCurrently(FsmState state)
        {
            return state == CurrentState;
        }

        // Update our current state provided that it is distinct from the next
        protected void MoveToState(FsmState next)
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
            CurrentState = _nextScheduledState;
            _nextScheduledState = null;
            return true;
        }

        /*** Tnternal Hooks to MonoBehavior ***/

        private void Awake()
        {
            Initialize(InitialState);
            if (InitialState == null)
            {
                throw new InvalidOperationException("InitialState is null - " +
                    "base initialize must be called within subclass initialize");
            }

            CurrentState = InitialState;
        }

        private void Start()
        {
            CurrentState.Enter();
        }

        private void Update()
        {
            bool hasEnteredNewStateThisFrame = ExecuteTransitionIfPending();
            if (!hasEnteredNewStateThisFrame)
            {
                CurrentState.Update();
            }
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

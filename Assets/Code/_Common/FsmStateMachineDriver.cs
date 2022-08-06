using System;
using UnityEngine;


namespace PQ.Common
{
    /*
    Driver for state machine that hooks up state callbacks to MonoBehaviour.

    Note that there are no transitional checks - it's entirely up to the child class implementation to
    determine when transitions should occur.
    */
    public abstract class FsmStateMachineDriver<TEnum> : MonoBehaviour
        where TEnum : Enum
    {
        protected FsmState<TEnum> InitialState { get; private set; }
        protected FsmState<TEnum> CurrentState { get; private set; }

        // Initialization method that MUST be overridden in subclasses; don't forget base.Initialize(initialState)
        protected virtual void Initialize(FsmState<TEnum> initialState) => InitialState = initialState;

        // todo: replace with event integration, and/or per frame update with priorities to ensure correct ordering
        // Required method implementation, where transitions are checked and called
        //protected abstract void ExecuteAnyTransitions();
        
        // Optional overridable callback for state transitions
        protected virtual void OnTransition(FsmState<TEnum> previous, FsmState<TEnum> next) { }

        // Is this our current state in the FSM?
        protected bool IsCurrently(FsmState<TEnum> state)
        {
            return state == CurrentState;
        }

        // Update our current state provided that it is distinct from the next
        protected void MoveToState(FsmState<TEnum> next)
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

            FsmState<TEnum> previous = CurrentState;
            previous.Exit();
            OnTransition(previous, next);
            next.Enter();
            CurrentState = next;
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
            CurrentState.Update();
            //ExecuteAnyTransitions();
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

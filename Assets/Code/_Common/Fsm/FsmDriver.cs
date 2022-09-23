using System;
using System.Collections.Generic;
using UnityEngine;


namespace PQ.Common.Fsm
{
    /*
    Driver for state machine that hooks up state callbacks to MonoBehaviour.

    Note that while there is transition validation, the conditions that trigger those
    state changes are up to the specific fsm state implementation.


    Note that exceptions are thrown EARLY for invalid driver setup, meaning that if it made it past start
    with no exceptions, we have unique keys, no nulls in the graph, non empty graphs, explicitly set initial state,
    and other invariants.

    By doing so as early as Start() being called, this means that we can catch a multitude of developer errors
    effectively on game load, rather than much later on during game execution.
    */
    public abstract class FsmDriver<T> : MonoBehaviour
        where T : FsmBlackboardData
    {
        private bool _initialized;
        private FsmGraph<T> _fsmGraph;
        private FsmBlackboard<T> _fsmBlackboard;

        private FsmState<T> _initial;
        private FsmState<T> _current;
        private FsmState<T> _last;
        private FsmState<T> _next;

        public override string ToString() =>
            $"FsmDriver:{{" +
                $"\nFsmData({_fsmBlackboard}), " +
                $"\nFsmHistory(" +
                    $"initial:{_initial.Id}," +
                    $"current:{_current.Id}," +
                    $"last:{_last.Id}," +
                    $"next:{_next.Id})" +
                $"{_fsmGraph}" +
            $"}}";



        /*** Internal Hooks for Setting up a Specific State Machine Instance ***/

        public sealed class Builder
        {
            public readonly T blob;
            public readonly string initial;
            public readonly List<(FsmState<T>, string[])> nodes;

            public Builder(T persistentData, string initial)
            {
                this.blob = persistentData;
                this.initial = initial;
                this.nodes = new List<(FsmState<T>, string[])>();
            }

            public Builder AddNode<StateImpl>(string id, string[] transitions)
                where StateImpl : FsmState<T>, new()
            {
                nodes.Add((FsmState<T>.Create<StateImpl>(id, blob), transitions));
                return this;
            }
        }

        // Sole source of truth for specifying blackboard data, initial state, and allowed transitions
        // Strictly required to be invoked only once and only in OnInitialize()
        protected void Initialize(Builder builder)
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"Cannot initialize - blob and graph were already set");
            }
            if (builder == null)
            {
                throw new InvalidOperationException($"Cannot initialize - builder cannot be null");
            }

            _initialized   = true;
            _fsmGraph      = new FsmGraph<T>(builder.nodes);
            _fsmBlackboard = new FsmBlackboard<T>(builder.blob);
            _initial       = _fsmGraph.GetState(builder.initial);
            if (_initial == null)
            {
                throw new InvalidOperationException($"Cannot initialize - initial state {_initial.Id} was not found");
            }
        }


        // Required callback for initializing
        protected abstract void OnInitialize();

        // Optional overridable callback for after initializing and entering first state
        protected virtual void OnInitialStateEntered(string initial) { }

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(string source, string dest) { }



        /*** Internal Hooks to MonoBehavior ***/

        private void Start()
        {
            // since states may have may game object dependencies, we explicitly want to
            // initialize our fsm on start, rather in awake, where those objects may not be fully initialized
            OnInitialize();
            if (!_initialized)
            {
                throw new InvalidOperationException("Cannot start driver - blob and graph must be provided in OnInitialize()");
            }
            if (_next != null)
            {
                throw new InvalidOperationException("Cannot start driver - states can only be scheduled when driver is active!");
            }
            Enter(_initial);
            OnInitialStateEntered(_initial.Id);
        }

        private void Update()
        {
            ProcessTransitionIfScheduled();
            _current.Update();
        }

        private void FixedUpdate()
        {
            _current.FixedUpdate();
        }

        private void LateUpdate()
        {
            _current.LateUpdate();
        }



        /*** Internal 'Machinery' ***/

        private void HandleOnMoveToLastStateSignaled()            => ScheduleTransition(_last.Id);
        private void HandleOnMoveToNextStateSignaled(string dest) => ScheduleTransition(dest);

        private void ScheduleTransition(string dest)
        {
            if (_next != null)
            {
                throw new InvalidOperationException($"Cannot move to {dest} - a transition {_current.Id}=>{_next.Id} is already queued");
            }

            FsmState<T> next = _fsmGraph.GetState(dest);
            if (next == null)
            {
                throw new InvalidOperationException($"Cannot move to {dest} - state {next.Id} was not found");
            }
            if (!_fsmGraph.HasTransition(_current?.Id, dest))
            {
                throw new InvalidOperationException($"Cannot move to {dest} - transition {_current.Id}=>{dest} was not found");
            }

            _next = next;
        }

        private bool ProcessTransitionIfScheduled()
        {
            string source = _current?.Id;
            string dest   = _next?.Id;
            if (string.IsNullOrEmpty(dest))
            {
                return false;
            }

            Exit(_current);
            OnTransition(source, dest);
            Enter(_next);
            return true;
        }


        private void Exit(FsmState<T> state)
        {
            state.Exit();
            state.OnMoveToLastStateSignaled.RemoveHandler(HandleOnMoveToLastStateSignaled);
            state.OnMoveToNextStateSignaled.RemoveHandler(HandleOnMoveToNextStateSignaled);
            _last    = state;
            _current = null;
        }

        private void Enter(FsmState<T> state)
        {
            state.Enter();
            state.OnMoveToLastStateSignaled.AddHandler(HandleOnMoveToLastStateSignaled);
            state.OnMoveToNextStateSignaled.AddHandler(HandleOnMoveToNextStateSignaled);
            _current = state;
            _next    = null;
        }
    }
}

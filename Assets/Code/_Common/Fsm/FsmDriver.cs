using PQ.Entities.Penguin;
using System;
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
        protected T Blob { get; set; }

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

        protected Instance CreateState<Instance>(string id)
            where Instance : FsmState<T>, new()
        {
            return FsmState<T>.Create<Instance>(id, Blob);
        }

        // Sole source of truth for specifying blackboard data, initial state, and allowed transitions
        // Strictly required to be invoked only once and only in OnInitialize()
        protected void Initialize(T blob, string startAt, params (FsmState<T>, string[])[] states)
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"Cannot initialize - blob and graph were already set");
            }

            var graph = new FsmGraph<T>(states);
            var initial = graph.GetState(startAt);
            var blackboard = new FsmBlackboard<T>(blob);
            if (initial == null)
            {
                throw new InvalidOperationException($"Cannot initialize - initial state {startAt} was not found");
            }

            Blob = blob;
            _initial = initial;
            _fsmGraph = graph;
            _fsmBlackboard = blackboard;
            _initialized = true;
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

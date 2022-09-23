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
        private FsmState<T> _initial;
        private FsmState<T> _current;
        private FsmState<T> _last;
        private FsmState<T> _next;

        private FsmGraph<T> _fsmGraph;
        private FsmBlackboard<T> _blackboard;
        protected abstract T Data { get; set; }


        /*** External Facing Methods Used to Drive Transitions ***/

        public string InitialState => _initial.Id;
        public string CurrentState => _current.Id;
        public string LastState    => _last.Id;
        public string NextState    => _next.Id;

        public override string ToString() =>
            $"FsmDriver:{{" +
                $"\nFsmData({_blackboard}), " +
                $"\nFsmHistory(" +
                    $"initial:{InitialState}," +
                    $"current:{CurrentState}," +
                    $"last:{LastState}," +
                    $"next:{NextState})" +
                $"{_fsmGraph}" +
            $"}}";


        /*** Internal Hooks for Setting up a Specific State Machine Instance ***/

        protected Instance CreateState<Instance>(string id)
            where Instance : FsmState<T>, new()
        {
            return FsmState<T>.Create<Instance>(id, Data);
        }

        // Sole source of truth for specifying the fsm states, initial state, and their possible transitions
        // Strictly required to be invoked only once and only in OnInitialize()
        protected void InitializeGraph(string initial, params (FsmState<T>, string[])[] states)
        {
            if (_fsmGraph != null)
            {
                throw new InvalidOperationException($"Cannot override graph - fsm graph already initialized");
            }
            _fsmGraph = new FsmGraph<T>(states);

            if (!_fsmGraph.TryGetState(initial, out FsmState<T> initialState))
            {
                throw new InvalidOperationException($"Cannot set initial state to {initial} - was not found");
            }
            _initial = initialState;
        }

        // Sole source of truth for specifying the access point for our data, sort of like a blackboard
        // Strictly required to be invoked only once and only in OnInitialize()
        protected void SetBlackboardData(T blackboardData)
        {
            if (_blackboard != null)
            {
                throw new InvalidOperationException($"Cannot override fsm blackboard data to" +
                    $"{blackboardData.name} - data already set");
            }

            _blackboard = new FsmBlackboard<T>(blackboardData);
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
            // initialize our fsm on start, rather in awake, where those objects may not fully initialized.
            //
            // note that post initialize we strictly enforce variants that should of been adhered to by
            // subclass implementation of OnInitialize()
            //
            OnInitialize();

            if (_fsmGraph == null)
            {
                throw new InvalidOperationException("Cannot start driver - graph must be populated in OnInitialize()");
            }
            if (_blackboard == null)
            {
                throw new InvalidOperationException("Cannot start driver - reference to fsm data must be set in OnInitialize()");
            }
            if (_initial == null)
            {
                throw new InvalidOperationException("Cannot start driver - initial state must be set in OnInitialize()");
            }

            if (_next != null)
            {
                throw new InvalidOperationException("Cannot start driver - states can only be scheduled when driver is active!");
            }

            Enter(_initial);
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

        // Update our current state if transition was previously registered during initialization
        private void ScheduleTransition(string dest)
        {
            if (_next != null)
            {
                throw new InvalidOperationException($"Cannot move to {dest} - a transition {_current.Id}=>{_next.Id} is already queued");
            }

            if (!_fsmGraph.TryGetState(dest, out FsmState<T> next))
            {
                throw new InvalidOperationException($"Cannot move to {dest} - state {next.Id} was not found");
            }
            if (!_fsmGraph.HasTransition(_current?.Id, dest))
            {
                throw new InvalidOperationException($"Cannot move to {dest} - transition {_current.Id}=>{dest} was not found");
            }

            _next = next;
        }

        // Update our current state provided that it is distinct from the next
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

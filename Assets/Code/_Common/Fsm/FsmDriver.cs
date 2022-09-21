using System;
using UnityEngine;


namespace PQ.Common.Fsm
{
    /*
    Driver for state machine that hooks up state callbacks to MonoBehaviour.

    Note that while there is transition validation, the conditions that trigger those
    state changes are up to the specific fsm state implementation.
    */
    public abstract class FsmDriver : MonoBehaviour
    {
        private FsmState _initial;
        private FsmState _current;
        private FsmState _last;
        private FsmState _next;

        private FsmGraph _fsmGraph;


        /*** External Facing Methods Used to Drive Transitions ***/

        public string InitialState => _initial.Id;
        public string CurrentState => _current.Id;
        public string LastState    => _last.Id;
        public string NextState    => _next.Id;

        public override string ToString() =>
            $"FsmDriver:{{" +
                $"\nFsmHistory(" +
                    $"initial:{InitialState}," +
                    $"current:{CurrentState}," +
                    $"last:{LastState}," +
                    $"next:{NextState})" +
                $"{_fsmGraph}" +
            $"}}";


        /*** Internal Hooks for Defining State Specific Logic ***/

        // Sole source of truth for specifying the fsm states and their possible transitions
        // Strictly required to be invoked only once and only in OnInitialize()
        protected void InitializeGraph(params (FsmState, string[])[] states)
        {
            if (_fsmGraph != null)
            {
                throw new InvalidOperationException($"Cannot override graph - fsm graph already initialized");
            }

            _fsmGraph = new(states);
        }

        // Sole source of truth for specifying the initial state
        // Strictly required to be invoked only once and only in OnInitialize()
        protected void SetInitialState(string id)
        {
            if (_fsmGraph == null)
            {
                throw new InvalidOperationException($"Cannot set initial state to {id} - graph not yet initialized");
            }
            if (_initial != null)
            {
                throw new InvalidOperationException($"Cannot override initial state to {id} -  initial state already set");
            }
            if (!_fsmGraph.TryGetState(id, out FsmState initialState))
            {
                throw new InvalidOperationException($"Cannot set initial state to {id} -  was not found");
            }

            _initial = initialState;
        }

        // Required callback for initializing
        protected abstract void OnInitialize();

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(string source, string dest) { }



        /*** Internal Hooks to MonoBehavior ***/

        private void Start()
        {
            // since states may have may game object dependencies, we explicitly want to
            // initialize our fsm on start, rather in awake, where those objects may not fully initialized.
            OnInitialize();
            if (_fsmGraph == null)
            {
                throw new InvalidOperationException("Cannot start driver - graph must be populated in OnInitialize()");
            }
            if (_initial == null)
            {
                throw new InvalidOperationException("Cannot start driver - initial state must be set in OnInitialize()");
            }
            if (_next != null)
            {
                throw new InvalidOperationException("Cannot start driver - states can only be scheduled when driver is active");
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

            if (!_fsmGraph.TryGetState(dest, out FsmState next))
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


        private void Exit(FsmState state)
        {
            state.Exit();
            state.OnMoveToLastStateSignaled.RemoveHandler(HandleOnMoveToLastStateSignaled);
            state.OnMoveToNextStateSignaled.RemoveHandler(HandleOnMoveToNextStateSignaled);
            _last    = state;
            _current = null;
        }

        private void Enter(FsmState state)
        {
            state.Enter();
            state.OnMoveToLastStateSignaled.AddHandler(HandleOnMoveToLastStateSignaled);
            state.OnMoveToNextStateSignaled.AddHandler(HandleOnMoveToNextStateSignaled);
            _current = state;
            _next    = null;
        }
    }
}

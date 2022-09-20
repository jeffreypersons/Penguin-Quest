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


        // Update our current state if transition was previously registered during initialization
        public void MoveToState(string stateId)
        {
            if (_next != null)
            {
                throw new InvalidOperationException($"Cannot move to {stateId} - a transition {_current.Id}=>{_next.Id} is already queued");
            }
            if (!_fsmGraph.TryGetState(stateId, out FsmState next))
            {
                throw new InvalidOperationException($"Cannot move to {stateId} - state {next.Id} was not found");
            }
            if (!_fsmGraph.HasTransition(_current.Id, stateId))
            {
                throw new InvalidOperationException($"Cannot move to {stateId} - transition {_current.Id}=>{stateId} was not found");
            }

            _next = next;
        }



        /*** Internal Hooks for Defining State Specific Logic ***/

        // Initialization method that MUST be called in OnInitialize in subclasses

        // Mechanism for hooking up transitions use for validation when MoveToState is called by client
        // Can only be invoked in OnInitialize
        protected void InitializeGraph(params (FsmState, string[])[] states)
        {
            if (_fsmGraph != null)
            {
                throw new InvalidOperationException($"Cannot initialize graph - fsm graph already initialized");
            }

            _fsmGraph = new(states);
            SetInitialState(states[0].Item1.Id);
        }
        
        // Override the initial state
        protected void SetInitialState(string stateId)
        {
            if (_fsmGraph == null)
            {
                throw new InvalidOperationException($"Cannot set initial state to {stateId} - graph not yet initialized");
            }
            if (!_fsmGraph.TryGetState(stateId, out FsmState initialState))
            {
                throw new InvalidOperationException($"Cannot set initial state to {stateId} -  was not found");
            }
            _initial = initialState;
        }

        // Required callback for initializing
        protected abstract void OnInitialize();

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(string sourceId, string destinationId) { }



        /*** Internal Hooks to MonoBehavior ***/

        private void Start()
        {
            // since states may have may game object dependencies, we explicitly want to
            // initialize our fsm on start, rather in awake, where those objects may not fully initialized.
            OnInitialize();
            if (_fsmGraph == null)
            {
                throw new InvalidOperationException("Graph was not initialized - " +
                    "InitializeGraph must be called within subclass OnInitialize");
            }

            _current = _initial;
            _current.Enter();
        }

        private void Update()
        {
            ExecuteTransitionIfPending();
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


        // Update our current state provided that it is distinct from the next
        private bool ExecuteTransitionIfPending()
        {
            if (_next == null)
            {
                return false;
            }

            _current.Exit();
            OnTransition(_current.Id, _next.Id);
            _next.Enter();

            _last    = _current;
            _current = _next;
            _next    = null;
            return true;
        }
    }
}

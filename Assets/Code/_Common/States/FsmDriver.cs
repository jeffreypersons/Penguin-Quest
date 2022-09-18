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
        private bool _initialized = false;
        private FsmState _initial;
        private FsmState _current;
        private FsmState _last;
        private FsmState _next;

        private FsmGraph _fsmGraph;


        /*** External Facing Methods Used to Drive Transitions ***/

        public FsmState InitialState => _initial;
        public FsmState CurrentState => _current;
        public FsmState LastState    => _last;
        public FsmState NextState    => _next;

        public override string ToString() =>
            $"FsmDriver:{{" +
                $"\nFsmHistory(" +
                    $"initial:{InitialState.Name}," +
                    $"current:{CurrentState.Name}," +
                    $"last:{LastState.Name}," +
                    $"next:{NextState.Name})" +
                $"{_fsmGraph}" +
            $"}}";


        // Update our current state if transition was previously registered during initialization
        public void MoveToState(FsmState next)
        {
            if (_next != null)
            {
                throw new InvalidOperationException($"Cannot move to {next.Name} - a transition {_current.Name}=>{_next.Name} is already queued");
            }
            if (!_fsmGraph.HasTransition(_current.Name, next.Name))
            {
                throw new InvalidOperationException($"Cannot move to {next.Name} - transition {_current.Name}=>{next.Name} was not found");
            }

            _next = next;
        }



        /*** Internal Hooks for Defining State Specific Logic ***/

        // Initialization method that MUST be called in OnInitialize in subclasses
        protected void InitializeStates(params FsmState[] states)
        {
            _fsmGraph = new FsmGraph(states);

            _initial = states[0];
            for (int i = 0; i < states.Length; i++)
            {
                states[i].Initialize();
            }

            _initialized = true;
        }

        // Mechanism for hooking up transitions use for validation when MoveToState is called by client
        // Can only be invoked in OnInitialize
        protected void RegisterTransition(FsmState source, FsmState destination)
        {
            _fsmGraph.AddTransition(source.Name, destination.Name);
        }

        // Required callback for initializing
        protected abstract void OnInitialize();

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(FsmState previous, FsmState next) { }



        /*** Internal Hooks to MonoBehavior ***/

        private void Start()
        {
            // since states may have may game object dependencies, we explicitly want to
            // initialize our fsm on start, rather in awake, where those objects may not fully initialized.
            OnInitialize();
            if (!_initialized)
            {
                throw new InvalidOperationException("States were not initialized - " +
                    "InitializeStates must be called within subclass OnInitialize");
            }

            _current = InitialState;
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
            OnTransition(_current, _next);
            _next.Enter();

            _last = _current;
            _current = _next;
            _next = null;
            return true;
        }
    }
}

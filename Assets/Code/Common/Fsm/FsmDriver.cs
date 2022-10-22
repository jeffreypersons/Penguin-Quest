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
    public abstract class FsmDriver<StateId, SharedData> : MonoBehaviour
        where StateId    : struct, Enum
        where SharedData : FsmSharedData
    {
        private bool _initialized;
        private SharedData _sharedData;
        private FsmGraph<StateId, SharedData> _graph;

        private FsmState<StateId, SharedData> _initialState;
        private FsmState<StateId, SharedData> _previousState;
        private FsmState<StateId, SharedData> _activeState;
        private FsmState<StateId, SharedData> _scheduledState;

        public override string ToString()
        {
            return
                $"{GetType()}(gameObject:{base.name},data:{_sharedData})" +
                $"\n  FsmStateHistory(" +
                    $"current:{_activeState?.Name    ?? "<none>"}," +
                    $"initial:{_initialState?.Name   ?? "<none>"}," +
                    $"last:{   _previousState?.Name  ?? "<none>"}," +
                    $"next:{   _scheduledState?.Name ?? "<none>"})" +
                $"\n{_graph}";
        }


        /*** Internal Hooks for Setting up a Specific State Machine Instance ***/

        public sealed class Builder
        {
            public readonly StateId initial;
            public readonly SharedData data;
            public readonly List<(FsmState<StateId, SharedData>, StateId[])> nodes;

            public Builder(SharedData persistentData, StateId initial)
            {
                if (persistentData == null || !persistentData)
                {
                    throw new InvalidOperationException(
                        $"Cannot initialize - shared data cannot be null or destroyed," +
                        $"as otherwise any event registration in state.OnInitialize() will fail");
                }
                this.initial = initial;
                this.data    = persistentData;
                this.nodes   = new List<(FsmState<StateId, SharedData>, StateId[])>();
            }

            public Builder AddNode<StateSubclass>(StateId id, StateId[] transitions)
                where StateSubclass : FsmState<StateId, SharedData>, new()
            {
                nodes.Add((FsmState<StateId, SharedData>.Create<StateSubclass>(id, data), transitions));
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

            _initialized  = true;
            _graph        = new FsmGraph<StateId, SharedData>(builder.nodes);
            _sharedData   = builder.data;
            _initialState = _graph.GetState(builder.initial);

            if (_sharedData == null || !_sharedData)
            {
                throw new InvalidOperationException($"Cannot initialize - shared data cannot be null or destroyed");
            }
            if (_initialState == null)
            {
                throw new InvalidOperationException($"Cannot initialize - initial state {_initialState} was not found");
            }
        }


        // Required callback for initializing
        protected abstract void OnInitialize();

        // Optional overridable callback for after initializing and entering first state
        protected virtual void OnInitialStateEntered(StateId initial) { }

        // Optional overridable callback for state transitions
        protected virtual void OnTransition(StateId source, StateId dest) { }



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
            if (_scheduledState != null)
            {
                throw new InvalidOperationException("Cannot start driver - states can only be scheduled when driver is active!");
            }
            Enter(_initialState);
            OnInitialStateEntered(_initialState.Id);
        }

        private void FixedUpdate()
        {
            _activeState.ExecuteFixedUpdate();
        }

        private void OnAnimatorMove()
        {
            _activeState.ExecuteAnimatorRootMotionUpdate();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            _activeState.ExecuteAnimatorIkPassUpdate(layerIndex);
        }

        private void Update()
        {
            // todo: look into if transition processing would actually make more sense in fixed update,
            //       as that's around where the animator does it..
            //       see https://docs.unity3d.com/2022.1/Documentation/Manual/ExecutionOrder.html
            ProcessTransitionIfScheduled();
            _activeState.ExecuteUpdate();
        }

        private void LateUpdate()
        {
            _activeState.ExecuteLateUpdate();
        }



        /*** Internal 'Machinery' ***/

        private void ScheduleTransition(StateId dest)
        {
            if (_scheduledState != null)
            {
                throw new InvalidOperationException(
                    $"Cannot move to {dest} - a transition " +
                    $"{_activeState.Id}=>{_scheduledState.Id} is already queued");
            }
            if (!_graph.HasTransition(_activeState.Id, dest))
            {
                throw new InvalidOperationException(
                    $"Cannot move to {dest} - transition " +
                    $"{_activeState.Id}=>{dest} was not found");
            }

            _scheduledState = _graph.GetState(dest);
        }

        private bool ProcessTransitionIfScheduled()
        {
            if (_scheduledState == null)
            {
                return false;
            }

            StateId source = _activeState.Id;
            StateId dest   = _scheduledState.Id;

            Exit(_activeState);
            OnTransition(source, dest);
            Enter(_scheduledState);
            return true;
        }


        private void HandleOnMoveToPreviousStateSignaled()         => ScheduleTransition(_previousState.Id);
        private void HandleOnMoveToNextStateSignaled(StateId dest) => ScheduleTransition(dest);

        private void Exit(FsmState<StateId, SharedData> state)
        {
            state.Exit();
            state.OnMoveToPreviousStateSignaled.RemoveHandler(HandleOnMoveToPreviousStateSignaled);
            state.OnMoveToNextStateSignaled.RemoveHandler(HandleOnMoveToNextStateSignaled);
            _previousState = state;
            _activeState   = null;
        }

        private void Enter(FsmState<StateId, SharedData> state)
        {
            state.Enter();
            state.OnMoveToPreviousStateSignaled.AddHandler(HandleOnMoveToPreviousStateSignaled);
            state.OnMoveToNextStateSignaled.AddHandler(HandleOnMoveToNextStateSignaled);
            _activeState    = state;
            _scheduledState = null;
        }
    }
}

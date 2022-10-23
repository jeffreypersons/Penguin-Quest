using System;
using System.Collections.Generic;
using PQ.Common.Events;


namespace PQ.Common.Fsm
{
    /*
    Representation of a node in a finite state machine that encapsulates state-specific logic.


    Note that is it up to the call site to do any transition validation (or similar) - and that all states have a
    single unified entry point, with a blackboard instance (SharedData) that can be asked for data relevant to the
    specific state implementation.
    */
    public abstract class FsmState<StateId, SharedData>
        : IEquatable<FsmState<StateId, SharedData>>,
          IComparable<FsmState<StateId, SharedData>>
        where SharedData : FsmSharedData
        where StateId    : struct, Enum
    {
        private StateId          _id;
        private string           _name;
        private SharedData       _data;
        private bool             _active;
        private PqEventRegistry  _eventRegistry;
        private PqEvent          _moveToPreviousStateSignal = new("fsm.state.move.previous");
        private PqEvent<StateId> _moveToNextStateSignal     = new("fsm.state.move.next");

        public    StateId    Id     => _id;
        public    string     Name   => _name;
        protected SharedData Blob   => _data;
        public    bool       Active => _active;
        public IPqEventReceiver          OnMoveToPreviousStateSignaled => _moveToPreviousStateSignal;
        public IPqEventReceiver<StateId> OnMoveToNextStateSignaled     => _moveToNextStateSignal;

        public override string ToString() =>
            $"FsmState(" +
                $"id:{_id}, " +
                $"active:{_active}, " +
                $"blob:{_data}, " +
                $"eventRegistry:[{_eventRegistry}]" +
            $")";

        private static readonly Comparer<StateId>         idValueComparer;
        private static readonly EqualityComparer<StateId> idEqualityComparer;

        static FsmState()
        {
            // note that since enums are processed during compile time, we cache the comparers once per static id type,
            // since unfortunately we can't just == to compare generic enum structs
            idValueComparer    = Comparer<StateId>.Default;
            idEqualityComparer = EqualityComparer<StateId>.Default;
        }



        /*** External Facing Methods for Driving State Logic ***/

        public FsmState() { }


        public static StateSubclassInstance Create<StateSubclassInstance>(StateId id, SharedData blob)
            where StateSubclassInstance : FsmState<StateId, SharedData>, new()
        {
            StateSubclassInstance instance = new()
            {
                _id            = id,
                _name          = id.ToString(),
                _data          = blob,
                _active        = false,
                _eventRegistry = new(),
            };
            instance.OnIntialize();
            instance._eventRegistry.UnsubscribeToAllRegisteredEvents();
            return instance;
        }

        public void Enter()
        {
            OnEnter();
            _active = true;
            _eventRegistry.SubscribeToAllRegisteredEvents();
        }

        public void Exit()
        {
            OnExit();
            _active = false;
            _eventRegistry.UnsubscribeToAllRegisteredEvents();
        }

        // Execute logic intended for early through a frame such as fixed duration physics calculations
        public void ExecuteFixedUpdate()                        => OnFixedUpdate();

        // Execute logic intended for just after animator states and events are processed,
        // but before internal physics, ik and transform pass writes
        public void ExecuteAnimatorRootMotionUpdate()           => OnAnimatorRootMotionUpdate();

        // Execute logic intended for just after unity's internal physics updates and animation processed,
        // but before ik and previous physics transform pass writes (this may take multiple passes in a frame)
        public void ExecuteAnimatorIkPassUpdate(int layerIndex) => OnAnimatorIkPassUpdate(layerIndex);

        // Execute logic intended for mid way in a frame such as processing input
        public void ExecuteUpdate()                             => OnUpdate();

        // Execute logic intended for later on in a frame such as programmatic visual effects
        public void ExecuteLateUpdate()                         => OnLateUpdate();



        /*** Internal Hooks for Defining State Specific Logic ***/

        protected void SignalMoveToPreviousState()                                      => _moveToPreviousStateSignal.Raise();
        protected void SignalMoveToNextState(StateId dest)                              => _moveToNextStateSignal.Raise(dest);
        protected void RegisterEvent(IPqEventReceiver event_, Action handler_)          => _eventRegistry.Add(event_, handler_);
        protected void RegisterEvent<T>(IPqEventReceiver<T> event_, Action<T> handler_) => _eventRegistry.Add(event_, handler_);


        // Required one time callback where long living data can be hooked up (eg events/handlers)

        protected abstract void OnIntialize();


        // Required entry/exit point callbacks

        protected abstract void OnEnter();
        protected abstract void OnExit();

        // Optional recurring callbacks

        protected virtual void OnFixedUpdate()                        { }
        protected virtual void OnAnimatorRootMotionUpdate()           { }
        protected virtual void OnAnimatorIkPassUpdate(int layerIndex) { }
        protected virtual void OnUpdate()                             { }
        protected virtual void OnLateUpdate()                         { }


        public bool        HasSameId(StateId id)                  => idEqualityComparer.Equals(_id, id);
        public static bool HasSameId(StateId left, StateId right) => idEqualityComparer.Equals(left, right);

        int IComparable<FsmState<StateId, SharedData>>.CompareTo(FsmState<StateId, SharedData> other) => Compare(this, other);
        bool IEquatable<FsmState<StateId, SharedData>>.Equals(FsmState<StateId, SharedData> other)    => Equal(this, other);

        public override bool Equals(object obj) => Equal(this, obj as FsmState<StateId, SharedData>);
        public override int GetHashCode() => HashCode.Combine(idEqualityComparer.GetHashCode(Id));

        public static bool operator ==(FsmState<StateId, SharedData> left, FsmState<StateId, SharedData> right) =>  Equal(left, right);
        public static bool operator !=(FsmState<StateId, SharedData> left, FsmState<StateId, SharedData> right) => !Equal(left, right);


        private static bool Equal(FsmState<StateId, SharedData> left, FsmState<StateId, SharedData> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left is null)
            {
                return false;
            }
            if (right is null)
            {
                return false;
            }
            return ReferenceEquals(left.Blob, right.Blob) && idEqualityComparer.Equals(left.Id, right.Id);
        }

        private static int Compare(FsmState<StateId, SharedData> left, FsmState<StateId, SharedData> right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left is null)
            {
                return -1;
            }
            if (right is null)
            {
                return 1;
            }
            return idValueComparer.Compare(left.Id, right.Id);
        }
    }
}

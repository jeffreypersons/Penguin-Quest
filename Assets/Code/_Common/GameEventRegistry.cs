using System;
using System.Collections.Generic;


namespace PQ.Common
{
    /*
    Provides a way to subscribe/unsubscribe an event and their corresponding handler.

    Note that intentionally the only exposed manipulation of events is on an all or nothing basis.
    The idea is providing a mechanism to hook up existing events with existing handlers that
    can be periodically be turned on and off as a group.

    Assumptions
    - Events and callbacks are available for entire life time of the registry
    - Order registered (as of now) is not significant in any way
    */
    public class GameEventRegistry
    {
        // note that interface is required for contraviant storage of event/handler pairs
        private interface IEntry
        {
            public string EventName { get; }
            public string HandlerName { get; }
            public void Subscribe();
            public void Unsubscribe();
        }

        private class EventActionEntry<T> : IEntry
        {
            private GameEvent<T> _event;
            private Action<T> _handler;

            string IEntry.EventName     => _event.Name;
            string IEntry.HandlerName   => _handler.Method.Name;
            void   IEntry.Subscribe()   => _event.AddListener(_handler);
            void   IEntry.Unsubscribe() => _event.RemoveListener(_handler);

            public EventActionEntry(GameEvent<T> event_, Action<T> handler_)
            {
                _event = event_;
                _handler = handler_;
            }
        }


        private bool _active;
        private string _description;
        private List<IEntry> _eventActionEntries;

        public bool IsActive => _active;
        public override string ToString() => _description == "" ? "<empty>" : _description;

        public GameEventRegistry()
        {
            _active = false;
            _description = "";
            _eventActionEntries = new();
        }

        public void SubscribeToAllRegisteredEvents()
        {
            _active = true;
            for (int i = 0; i < _eventActionEntries.Count; i++)
            {
                _eventActionEntries[i].Subscribe();
            }
        }

        public void UnsubscribeToAllRegisteredEvents()
        {
            _active = false;
            for (int i = 0; i < _eventActionEntries.Count; i++)
            {
                _eventActionEntries[i].Unsubscribe();
            }
        }

        public void Add<T>(GameEvent<T> event_, Action<T> handler_)
        {
            IEntry entry = new EventActionEntry<T>(event_, handler_);
            if (_eventActionEntries.Exists(e => e.EventName == entry.EventName))
            {
                throw new ArgumentException($"{event_.Name} is already in registry");
            }

            // explicitly enforce that any new event-handler pairs have a subscription state matching the
            // rest of the event-handler pairs in the registry
            if (_active)
            {
                entry.Subscribe();
            }
            else
            {
                entry.Unsubscribe();
            }

            _description += $"{entry.EventName}=>{entry.HandlerName};";
            _eventActionEntries.Add(entry);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;


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
        private class Entry<T>
            where T : IEventPayload
        {
            public string    EventName   { get; set; }
            public string    HandlerName { get; set; }
            public Action<T> Event       { get; set; }
            public Action<T> Handler     { get; set; }

            public void Subscribe()   => Event += Handler;
            public void Unsubscribe() => Event -= Handler;
            public override string ToString() => $"{EventName}=>{HandlerName};";

            private Entry(string eventName_, Action<T> event_, string handlerName_, Action<T> handler_)
            {

                EventName = eventName_;
                Event = event_;
                HandlerName = handlerName_;
                Handler = handler_;
            }

            public static Entry<T> From(GameEvent<T> event_, Action<T> handler_)
            {
                return new Entry<T>(event_.Name, event_.AsAction, handler_.Method.Name, handler_);
            }
        }


        private bool _active;
        private string _description;
        private List<Entry<IEventPayload>> _eventHandlers;

        public bool IsActive => _active;
        public override string ToString() => _description == ""? "<empty>" : _description;

        public GameEventRegistry()
        {
            _active = false;
            _description = "";
            _eventHandlers = new();
        }

        public void SubscribeToAllRegisteredEvents()
        {
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                _eventHandlers[i].Event += _eventHandlers[i].Handler;
            }
        }

        public void UnsubscribeToAllRegisteredEvents()
        {
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                _eventHandlers[i].Event -= _eventHandlers[i].Handler;
            }
        }

        public void Add<T>(GameEvent<T> event_, Action<T> handler_) where T : IEventPayload
        {
            var newEntry = Entry<T>.From(event_, handler_);
            if (IsEventAlreadyInRegistry(newEntry))
            {
                throw new ArgumentException($"{event_.Name} is already in registry");
            }

            _description += newEntry.ToString();

            _eventHandlers.Add(newEntry as Entry<IEventPayload>);

            // explicitly enforce that any new event-handler pairs have a subscription state matching the
            // rest of the event-handler pairs in the registry
        }

        private bool IsEventAlreadyInRegistry<T>(Entry<T> entry) where T : IEventPayload
        {
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                if (_eventHandlers[i].EventName == entry.EventName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

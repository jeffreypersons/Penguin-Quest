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
        private interface IEntry
        {
            public string Key { get; }
            public void Subscribe();
            public void Unsubscribe();
        }
        private class Entry<T> : IEntry
        {
            public string Key => Event.Name;
            public GameEvent<T> Event   { get; set; }
            public Action<T>    Handler { get; set; }

            public void Subscribe()   => Event.AddListener(Handler);
            public void Unsubscribe() => Event.RemoveListener(Handler);
            public override string ToString() => $"{Event.Name}=>{Handler.Method.Name};";

            public Entry(GameEvent<T> event_, Action<T> handler_)
            {
                Event = event_;
                Handler = handler_;
            }
        }


        private bool _active;
        private string _description;
        private List<IEntry> _eventHandlers;

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
                _eventHandlers[i].Subscribe();
            }
        }

        public void UnsubscribeToAllRegisteredEvents()
        {
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                _eventHandlers[i].Unsubscribe();
            }
        }

        public void Add<T>(GameEvent<T> event_, Action<T> handler_)
        {
            var newEntry = new Entry<T>(event_, handler_);
            if (IsEventAlreadyInRegistry(newEntry))
            {
                throw new ArgumentException($"{event_.Name} is already in registry");
            }

            // explicitly enforce that any new event-handler pairs have a subscription state matching the
            // rest of the event-handler pairs in the registry
            if (_active)
            {
                newEntry.Subscribe();
            }
            else
            {
                newEntry.Unsubscribe();
            }

            _description += newEntry.ToString();
            _eventHandlers.Add(newEntry);
        }

        private bool IsEventAlreadyInRegistry<T>(Entry<T> entry)
        {
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                if (_eventHandlers[i].Key == entry.Event.Name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

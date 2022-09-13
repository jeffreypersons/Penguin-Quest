/*
Lightweight event primitive for game wide usage that's used for triggering and receiving event payloads.


Intended to provide a unified 'single source of truth' for any game event, rather than an inconsistent mix
of C# delegates, animation events, ui events, input actions, and so forth.

Instead, by triggering a GameEvent when those specific external events occur - eg from an adapter class for
processing player input commands - we have a consistent interface for events throughout the rest of the game.


Features

- Equality: Equality and hashing are supported out of the box, so unlike actions they can be freely used as dictionary keys

- Stateless forwarding: Event data is forwarded to subscribers on trigger rather than stored within the instance

- Cache friendly: Since data is directly forwarded to subscribers, events only need to be newed up once, relieving the GC

- Intentionally restricted to a single param data payload made available to any subscribing actions

- Lightweight: Built on top of native C# event actions, so far more performant than eg UnityEngine.Event as there is
  no need for reflection, serializing, and constant polling in its implementation
*/
using System;


namespace PQ.Common
{
    /* Lightweight event primitive for game wide usage used for triggering parameter-less events. */
    public class Event : IEventRaiser, IEventHandler, IEquatable<Event>
    {
        private readonly string _name;
        private event Action _action = delegate { };

        public string Name => _name;
        public Event(string name) => _name = name;

        void IEventRaiser.Raise()                          => _action.Invoke();
        void IEventHandler.AddHandler(Action onTrigger)    => _action += onTrigger;
        void IEventHandler.RemoveHandler(Action onTrigger) => _action -= onTrigger;
        bool IEquatable<Event>.Equals(Event other)         => other is not null && Name == other.Name;

        public override string ToString()       => $"Event({_name})";
        public override bool Equals(object obj) => ((IEquatable<Event>)this).Equals(obj as Event);
        public override int GetHashCode()       => HashCode.Combine(base.GetHashCode(), _action.GetHashCode(), Name);

        public static bool operator ==(Event left, Event right) =>  Equals(left, right);
        public static bool operator !=(Event left, Event right) => !Equals(left, right);
    }


    /* Lightweight event primitive for game wide usage used for triggering parameter inclusive events. */
    public class Event<T> : IEventRaiser<T>, IEventHandler<T>, IEquatable<Event<T>>
    {
        private readonly string _name;
        private event Action<T> _action = delegate { };

        public string Name => _name;
        public Event(string name) => _name = name;

        void IEventRaiser<T> .Raise(T args)                      => _action.Invoke(args);
        void IEventHandler<T>.AddHandler(Action<T> onTrigger)    => _action += onTrigger;
        void IEventHandler<T>.RemoveHandler(Action<T> onTrigger) => _action -= onTrigger;
        bool IEquatable<Event<T>>.Equals(Event<T> other)         => other is not null && Name == other.Name;

        public override string ToString()       => $"Event<{typeof(T).FullName}>({_name})";
        public override bool Equals(object obj) => ((IEquatable<Event<T>>)this).Equals(obj as Event<T>);
        public override int GetHashCode()       => HashCode.Combine(base.GetHashCode(), _action.GetHashCode(), Name);

        public static bool operator ==(Event<T> left, Event<T> right) =>  Equals(left, right);
        public static bool operator !=(Event<T> left, Event<T> right) => !Equals(left, right);
    }
}

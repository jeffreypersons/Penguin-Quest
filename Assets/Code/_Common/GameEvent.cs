using System;


namespace PQ.Common
{
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

    /*
    Intended to define a readonly game event argument interface for any type.

    Intended to be implemented by readonly structs for performance reasons.
    */
    public interface IEventPayload
    {
        public struct Empty : IEventPayload
        {
            public static readonly Empty Value;
        }
    }

    public interface IGameEvent<in T> { }

    public class GameEvent<T> : IGameEvent<T>, IEquatable<GameEvent<T>>
    {
        private readonly string _name;
        private event Action<T> _action;

        public string Name => _name;

        public int ListenerCount => _action.GetInvocationList().Length;
        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{Name}," +
                $"arg:{typeof(T).FullName}}}";

        public GameEvent(string name)
        {
            _name = name;
            _action = delegate { };
        }

        public void Trigger(in T payload)
        {
            _action.Invoke(payload);
        }

        public void AddListener(Action<T> listener)
        {
            // to guarantee we never hook a listener up to the same event more than once,
            // we ensure that we always unsubscribe in the case it already is before adding it
            _action -= listener;
            _action += listener;
        }

        public void RemoveListener(Action<T> listener)
        {
            _action -= listener;
        }

        bool IEquatable<GameEvent<T>>.Equals(GameEvent<T> other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => ((IEquatable<GameEvent<T>>)this).Equals(obj as GameEvent<T>);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _action.GetHashCode(), _name);
        public static bool operator ==(GameEvent<T> left, GameEvent<T> right) =>  Equals(left, right);
        public static bool operator !=(GameEvent<T> left, GameEvent<T> right) => !Equals(left, right);
    }
}

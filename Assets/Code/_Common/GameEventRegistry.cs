using System;
using System.Collections.Generic;
using System.Text;


namespace PQ.Common
{
    public class GameEventRegistry
    {
        private string _description;
        

        private readonly Dictionary<GameEvent<IEventPayload>, Action<IEventPayload>> _eventToHandlerMapping;
        public override string ToString() => _description;

        public GameEventRegistry(params (GameEvent<IEventPayload>, Action<IEventPayload>)[] eventCallbacks)
        {
            var stringBuilder = new StringBuilder(eventCallbacks.Length);
            _eventToHandlerMapping = new(eventCallbacks.Length);
            foreach (var (event_, callback_) in eventCallbacks)
            {
                _eventToHandlerMapping[event_] = callback_;
                stringBuilder.AppendFormat("{0}=>{1};", event_.Name, callback_.Method.Name);
            }

            _description = stringBuilder.ToString();
        }

        public void Add(GameEvent<IEventPayload> event_, Action<IEventPayload> callback_)
        {
            if (_eventToHandlerMapping.TryAdd(event_, callback_))
            {
                throw new ArgumentException("Event ");
            }
            _eventToHandlerMapping[event_] = callback_;
        }

        public void StartListening()
        {
            foreach (var (event_, callback_) in _eventToHandlerMapping)
            {
                event_.AddListener(callback_);
            }
        }

        public void StopListening()
        {
            foreach (var (event_, callback_) in _eventToHandlerMapping)
            {
                event_.RemoveListener(callback_);
            }
        }
    }
}

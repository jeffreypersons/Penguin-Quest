using System;
using System.Collections.Generic;
using System.Text;


namespace PQ.Common
{
    public class GameEventRegistry<EventData>
    {
        private string _description;
        private readonly Dictionary<GameEvent<EventData>, Action<EventData>> _eventToHandlerMapping;

        public override string ToString() => _description;


        public GameEventRegistry(params (GameEvent<EventData>, Action<EventData>)[] eventCallbacks)
        {
            var stringBuilder = new StringBuilder(eventCallbacks.Length);
            _eventToHandlerMapping = new Dictionary<GameEvent<EventData>, Action<EventData>>(eventCallbacks.Length);
            foreach (var (event_, callback_) in eventCallbacks)
            {
                _eventToHandlerMapping[event_] = callback_;
                stringBuilder.AppendFormat("{0}=>{1};", event_.Name, callback_.Method.Name);
            }

            _description = stringBuilder.ToString();
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

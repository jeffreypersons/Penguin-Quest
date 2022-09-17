using System;
using System.Collections.Generic;
using PQ.Common.Text;


namespace PQ.Common.States
{
    /*
    Provides info about the state machine - the states, history.
    */
    public class FsmGraph
    {
        private bool _isDirty;
        private string _description;
        private readonly FsmState[] _states;
        private Dictionary<string, HashSet<string>> _adjacencies;

        public FsmGraph(params FsmState[] states)
        {
            int stateCount = states.Length;
            if (stateCount == 0)
            {
                throw new ArgumentException("Fsm must have at least one state - received none");
            }

            _isDirty = true;
            _description = string.Empty;
            _states = new FsmState[states.Length];
            _adjacencies = new Dictionary<string, HashSet<string>>(stateCount);
            for (int i = 0; i < stateCount; i++)
            {
                FsmState state = states[i];
                if (state == null || string.IsNullOrEmpty(state.Name))
                {
                    throw new ArgumentNullException($"Cannot add null or empty named state to graph");
                }
                if (_adjacencies.ContainsKey(state.Name))
                {
                    throw new ArgumentException($"Cannot add {state.Name} state to graph - already exists");
                }

                _states[i] = state;
                _adjacencies[state.Name] = new HashSet<string>(stateCount);
            }
        }

        public FsmState LookupState(string name)
        {
            // todo: replace with more efficient lookup
            return Array.Find(_states, state => state.Name == name);
        }

        public bool HasTransition(string source, string destination)
        {
            return _adjacencies.ContainsKey(source) &&
                   _adjacencies[source].Contains(destination);
        }

        public void AddTransition(string source, string destination)
        {
            if (!_adjacencies.ContainsKey(source) || !_adjacencies.ContainsKey(destination))
            {
                throw new ArgumentException($"Cannot add transition {source}=>{destination} to graph - both states must exist");
            }
            if (source == destination)
            {
                throw new ArgumentException($"Cannot add transition {source}=>{destination} to graph - cannot loop to state");
            }
            if (_adjacencies[source].Contains(destination))
            {
                throw new ArgumentException($"Cannot add transition {source}=>{destination} to graph - already exists");
            }

            _adjacencies[source].Add(destination);
        }


        private const string Indent1 = "\n  ";
        private const string Indent2 = "\n    ";

        public override string ToString()
        {
            if (!_isDirty)
            {
                return _description;
            }

            FormattedList statesList      = new(start: $"{Indent1}states:",      end: "\n", sep: Indent2);
            FormattedList transitionsList = new(start: $"{Indent1}transitions:", end: "\n", sep: Indent2);
            for (int stateIndex = 0; stateIndex < _states.Length; stateIndex++)
            {
                FsmState state = _states[stateIndex];
                FormattedList stateTransitions = new($"{state.Name}:[", "]", ",", _adjacencies[state.Name]);

                statesList.Append(state.ToString());
                transitionsList.Append(stateTransitions.ToString());
            }

            _isDirty = false;
            _description =
                $"FsmGraph(" +
                    $"{statesList}" +
                    $"{transitionsList}" +
                $")";
            return _description;
        }
    }
}

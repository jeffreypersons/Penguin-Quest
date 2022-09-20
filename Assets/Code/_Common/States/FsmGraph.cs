using System;
using System.Text;
using System.Collections.Generic;


namespace PQ.Common.States
{
    /*
    Provides info about the state machine - the states, history.
    */
    internal class FsmGraph
    {
        private sealed class Node
        {
            public readonly FsmState state;
            public readonly HashSet<string> neighbors;

            public Node(FsmState state, HashSet<string> neighbors)
            {
                this.state = state;
                this.neighbors = neighbors;
            }
        }

        private readonly int _nodeCount;
        private readonly int _edgeCount;
        private readonly string _description;
        private readonly Dictionary<string, Node> _nodes;
        
        public override string ToString() => _description;

        /* Fill the graph with states, initialize them, and add their neighbors. */
        public FsmGraph(params (FsmState, string[])[] states)
        {
            if (states == null || states.Length == 0)
            {
                throw new ArgumentException("Fsm must have at least one state - received none");
            }

            // fill in the state ids first, so we can use for validating the rest of the input
            // when populating the graph states and transitions
            _nodes = new Dictionary<string, Node>(states.Length);
            foreach ((FsmState state, string[] _) in states)
            {
                string stateId = state?.Id;
                if (string.IsNullOrEmpty(stateId) || _nodes.ContainsKey(stateId))
                {
                    throw new ArgumentException($"Cannot add {stateId} state to graph - expected non null unique key");
                }
                _nodes.Add(key: stateId, value: null);
            }
            
            _nodeCount = 0;
            _edgeCount = 0;
            StringBuilder nodesInfo = new($" states");
            StringBuilder edgesInfo = new($" transitions");
            foreach ((FsmState state, string[] destinations) in states)
            {
                string source = state.Id;
                HashSet<string> neighbors = new(destinations.Length);
                foreach (string dest in destinations)
                {
                    if (source == dest || neighbors.Contains(dest) || !_nodes.ContainsKey(dest))
                    {
                        throw new ArgumentException($"Cannot add transition {source}=>{dest} to graph - expected unique existing key");
                    }
                    neighbors.Add(dest);
                }

                state.Initialize();
                nodesInfo.Append($"   ").AppendLine(state.ToString());
                edgesInfo.Append($"   {source} => {{").AppendJoin(",", neighbors).Append($"}}").AppendLine();

                _nodeCount++;
                _edgeCount += neighbors.Count;
                _nodes[source] = new(state, neighbors);
            }

            _description = $"FsmGraph({_nodeCount} states, {_edgeCount} transitions) \n{nodesInfo} \n{edgesInfo}";
        }

        public bool TryGetState(string id, out FsmState state)
        {
            if (!_nodes.ContainsKey(id))
            {
                state = null;
                return false;
            }

            state = _nodes[id].state;
            return true;
        }

        public bool HasState(string id)
        {
            return _nodes.ContainsKey(id);
        }

        public bool HasTransition(string source, string destination)
        {
            return _nodes.ContainsKey(source) &&
                   _nodes[source].neighbors.Contains(destination);
        }
    }
}

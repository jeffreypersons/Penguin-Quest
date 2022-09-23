using System;
using System.Text;
using System.Collections.Generic;


namespace PQ.Common.Fsm
{
    /*
    Representation of the states and transitions in a finite state machine.

    Note that it's effectively readonly - no new states or transitions after construction.

    Also, states cannot have edges that loop directly back to itself.
    */
    internal sealed class FsmGraph<T>
        where T : FsmBlackboardData
    {
        private sealed class Node
        {
            public readonly FsmState<T> state;
            public readonly HashSet<string> neighbors;

            public Node(FsmState<T> state, HashSet<string> neighbors)
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
        public FsmGraph(params (FsmState<T>, string[])[] states)
        {
            if (states == null || states.Length == 0)
            {
                throw new ArgumentException("Fsm must have at least one state - received none");
            }

            // fill in the state ids first, so we can use for validating the rest of the input
            // when populating the graph states and transitions
            _nodes = new Dictionary<string, Node>(states.Length);
            foreach ((FsmState<T> state, string[] _) in states)
            {
                string id = state?.Id;
                if (string.IsNullOrEmpty(id) || _nodes.ContainsKey(id))
                {
                    throw new ArgumentException($"Cannot add state {id} to graph - expected non null unique key");
                }
                _nodes.Add(id, null);
            }
            
            _nodeCount = 0;
            _edgeCount = 0;
            StringBuilder nodesInfo = new($" states");
            StringBuilder edgesInfo = new($" transitions");
            foreach ((FsmState<T> state, string[] destinations) in states)
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

        public bool HasState(string id) =>
            _nodes.ContainsKey(id);
        public bool HasTransition(string source, string dest) =>
            _nodes.ContainsKey(source) && _nodes[source].neighbors.Contains(dest);

        public FsmState<T> GetState(string id) => _nodes.ContainsKey(id) ? _nodes[id].state : null;
    }
}

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
    internal sealed class FsmGraph<StateId, SharedData>
        where StateId    : Enum
        where SharedData : FsmSharedData
    {
        private sealed class Node
        {
            public readonly FsmState<StateId, SharedData> state;
            public readonly HashSet<StateId> neighbors;

            public Node(FsmState<StateId, SharedData> state, HashSet<StateId> neighbors)
            {
                this.state = state;
                this.neighbors = neighbors;
            }
        }

        private readonly Type _idType;
        private readonly int _nodeCount;
        private readonly int _edgeCount;
        private readonly string _description;
        private readonly Dictionary<StateId, Node> _nodes;
        
        public override string ToString() => _description;

        /* Fill the graph with states, initialize them, and add their neighbors. */
        public FsmGraph(List<(FsmState<StateId, SharedData>, StateId[])> states)
        {
            if (states == null || states.Count == 0)
            {
                throw new ArgumentException("Fsm must have at least one state - received none");
            }

            // fill in the state ids first, so we can use for validating the rest of the input
            // when populating the graph states and transitions
            Type idType = typeof(StateId);
            _nodes = new Dictionary<StateId, Node>(states.Count);
            foreach ((FsmState<StateId, SharedData> state, StateId[] _) in states)
            {
                StateId id = state.Id;
                if (!Enum.IsDefined(idType, id) || _nodes.ContainsKey(id))
                {
                    throw new ArgumentException($"Cannot add state {id} to graph - expected non null unique key");
                }
                _nodes.Add(id, null);
            }

            _idType = typeof(StateId);
            _nodeCount = 0;
            _edgeCount = 0;
            StringBuilder nodesInfo = new($" states");
            StringBuilder edgesInfo = new($" transitions");
            foreach ((FsmState<StateId, SharedData> state, StateId[] destinations) in states)
            {
                StateId source = state.Id;
                HashSet<StateId> neighbors = new(destinations.Length);
                foreach (StateId dest in destinations)
                {
                    if (FsmState<StateId, SharedData>.HasSameId(source, dest) ||
                        neighbors.Contains(dest) ||
                        !_nodes.ContainsKey(dest))
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

        public bool HasState(StateId id) =>
            _nodes.ContainsKey(id);
        public bool HasTransition(StateId source, StateId dest) =>
            _nodes.ContainsKey(source) && _nodes[source].neighbors.Contains(dest);

        public FsmState<StateId, SharedData> GetState(StateId id) =>
            Enum.IsDefined(_idType, id) && _nodes.ContainsKey(id) ? _nodes[id].state : null;
    }
}

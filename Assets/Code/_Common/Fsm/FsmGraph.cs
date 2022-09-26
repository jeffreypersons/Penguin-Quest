using System;
using System.Text;
using System.Collections.Generic;
using PQ.Common.Extensions;


namespace PQ.Common.Fsm
{
    /*
    Representation of the states and transitions in a finite state machine.

    Note that it's effectively readonly - no new states or transitions after construction.

    Also, states cannot have edges that loop directly back to itself.
    */
    internal sealed class FsmGraph<StateId, SharedData>
        where StateId    : struct, Enum
        where SharedData : FsmSharedData
    {
        private sealed class Node
        {
            public readonly FsmState<StateId, SharedData> state;
            public readonly StateId neighborsBitset;

            public Node(FsmState<StateId, SharedData> state, StateId neighborsBitset)
            {
                this.state = state;
                this.neighborsBitset = neighborsBitset;
            }
        }

        private readonly int _nodeCount;
        private readonly int _edgeCount;
        private readonly string _description;
        private readonly Dictionary<StateId, Node> _nodes;

        public int StateCount => _nodeCount;
        public int TransitionCount => _edgeCount;

        private readonly string indent = new(' ', 4);
        
        public override string ToString() => _description;

        /* Fill the graph with states, initialize them, and add their neighbors. */
        public FsmGraph(in List<(FsmState<StateId, SharedData>, StateId[])> states)
        {
            if (states == null || states.Count == 0)
            {
                throw new ArgumentException("Fsm must have at least one state - received none");
            }

            // fill in the state ids first, so we can use for validating the rest of the input
            // when populating the graph states and transitions
            StringBuilder stringBuilder = new();
            _nodes = new Dictionary<StateId, Node>(states.Count);
            foreach ((FsmState<StateId, SharedData> state, StateId[] neighbors) in states)
            {
                stringBuilder.Append($"{indent}{state.Name}=>{{").AppendJoin(',', neighbors);
                StateId id = state.Id;
                if (_nodes.ContainsKey(id))
                {
                    throw new ArgumentException($"Cannot add state {id} to graph - expected non null unique key");
                }
                _nodes.Add(id, null);
            }

            _nodeCount = 0;
            _edgeCount = 0;
            foreach ((FsmState<StateId, SharedData> state, StateId[] destinations) in states)
            {
                StateId source = state.Id;
                StateId neighborBitset = default;
                int neighborCount = destinations.Length;

                foreach (StateId dest in destinations)
                {
                    stringBuilder.Append($"{EnumExtensions.NameOf(dest)},");

                    if (FsmState<StateId, SharedData>.HasSameId(source, dest) ||
                        EnumExtensions.HasFlags(neighborBitset, dest) ||
                        !_nodes.ContainsKey(dest))
                    {
                        throw new ArgumentException($"Cannot add transition {source}=>{dest} to graph - expected unique existing key");
                    }
                    neighborBitset = EnumExtensions.SetFlags(neighborBitset, dest);
                }
                RemoveTrailingCharacter(stringBuilder, ',');

                state.Initialize();
                _nodeCount++;
                _edgeCount += destinations.Length;
                _nodes[source] = new(state, neighborBitset);
            }

            _description = $"{{\n{stringBuilder}}}";
        }

        public bool HasState(StateId id) =>
            _nodes.ContainsKey(id);
        public bool HasTransition(StateId source, StateId dest) =>
            _nodes.ContainsKey(source) && EnumExtensions.HasFlags(_nodes[source].neighborsBitset, dest);

        public FsmState<StateId, SharedData> GetState(StateId id) =>
            _nodes.ContainsKey(id) ? _nodes[id].state : null;


        private static void RemoveTrailingCharacter(StringBuilder stringBuilder, char character)
        {
            int size = stringBuilder.Length;
            if (size > 0 && stringBuilder[size - 1] == character)
            {
                stringBuilder.Length--;
            }
        }
    }
}

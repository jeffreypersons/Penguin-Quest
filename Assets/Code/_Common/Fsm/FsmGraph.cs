using System;
using System.Text;
using System.Collections.Generic;
using PQ.Common.Containers;


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
        private struct Node
        {
            public readonly FsmState<StateId, SharedData> state;
            public readonly BitSet neighbors;
            public Node(FsmState<StateId, SharedData> state, BitSet neighbors)
            {
                this.state = state;
                this.neighbors = neighbors;
            }
        }

        public int StateCount      => _stateCount;
        public int TransitionCount => _transitionCount;
        public override string ToString() => _description;

        private readonly int    _stateCount;
        private readonly int    _transitionCount;
        private readonly string _description;
        private readonly Node[] _nodes;

        private static readonly string indentation;
        private static readonly FsmStateIdCache<StateId> idCache;
        static FsmGraph()
        {
            // note that since enums are processed during compile time, we resolve the cache only once per static id type
            indentation  = new(' ', 4);
            idCache = FsmStateIdCache<StateId>.Instance;
        }

        public FsmGraph(in List<(FsmState<StateId, SharedData>, StateId[])> adjacencyList)
        {
            if (adjacencyList == null || adjacencyList.Count == 0)
            {
                throw new ArgumentException($"Fsm must have at least one state - received none");
            }

            // note that since the nodes are created using bitset, node list match state id enum order
            _stateCount      = 0;
            _transitionCount = 0;
            _description     = string.Empty;
            _nodes           = ExtractNodeForEachDefinedId(adjacencyList);
            _description     = AsUserFriendlyString(_nodes);
        }

        public bool HasState(in StateId id) =>
            idCache.TryGetIndex(id, out _);

        public bool HasTransition(in StateId source, in StateId dest) =>
            idCache.TryGetIndex(source, out int sourceIndex) &&
            idCache.TryGetIndex(dest,   out int destIndex)   &&
            _nodes[sourceIndex].neighbors.HasIndex(destIndex);

        public FsmState<StateId, SharedData> GetState(in StateId id) =>
            idCache.TryGetIndex(id, out int index)? _nodes[index].state : null;


        private static Node[] ExtractNodeForEachDefinedId(
            in List<(FsmState<StateId, SharedData>, StateId[])> adjacencyList)
        {
            if (adjacencyList.Count != idCache.Count)
            {
                throw new ArgumentException($"Cannot extract nodes - " +
                    $"must have one state per stateId enum member yet counts are unequal");
            }

            // fill the ordered buckets according to their underlying ordinal type
            Node[] nodes = new Node[idCache.Count];
            foreach ((FsmState<StateId, SharedData> state, StateId[] adjacents) in adjacencyList)
            {
                if (state == null || adjacents == null)
                {
                    throw new ArgumentException($"Cannot add node - expected non null adjacency list entry");
                }

                StateId sourceId = state.Id;
                if (!idCache.TryGetIndex(state.Id, out int sourceIndex))
                {
                    throw new ArgumentException($"Cannot add node - {state.Id} is not a defined {typeof(StateId)} enum");
                }

                BitSet neighbors = new(idCache.Count);
                for (int i = 0; i < adjacents.Length; i++)
                {
                    StateId neighborId = adjacents[i];
                    if (!idCache.TryGetIndex(neighborId, out int neighborIndex))
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} -" +
                            $"destination is not a defined {typeof(StateId)} enum");
                    }
                    if (!neighbors.TryAdd(neighborIndex))
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} - expected unique existing key");
                    }
                    if (sourceIndex == neighborIndex)
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} - must be different states");
                    }
                }
                nodes[sourceIndex] = new Node(state, neighbors);
            }
            return nodes;
        }
        
        private static string AsUserFriendlyString(in Node[] nodes)
        {
            StringBuilder sb = new("{\n");
            foreach (Node node in nodes)
            {
                sb.Append($"{indentation}{node.state.Name} => {{");
                foreach ((int index, string name, StateId _) in idCache.Fields())
                {
                    if (node.neighbors.HasIndex(index))
                    {
                        sb.Append(name).Append(',');
                    }
                }
                RemoveTrailingCharacter(sb, ',');
                sb.Append($"}}\n");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static void RemoveTrailingCharacter(StringBuilder stringBuilder, char character)
        {
            int size = stringBuilder.Length;
            if (stringBuilder[size - 1] == character)
            {
                stringBuilder.Length--;
            }
        }
    }
}

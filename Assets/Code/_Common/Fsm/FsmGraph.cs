﻿using System;
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
    internal sealed class FsmGraph<Id, SharedData>
        where Id         : struct, Enum
        where SharedData : FsmSharedData
    {
        private static readonly FsmStateIdCache<Id> _stateIdCache = FsmStateIdCache<Id>.Instance;

        private struct Node
        {
            public readonly FsmState<Id, SharedData> state;
            public readonly BitSet neighbors;
            public Node(FsmState<Id, SharedData> state, BitSet neighbors)
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
        private static readonly string indentation = new(' ', 4);


        /* Fill the graph with states, initialize them, and add their neighbors. */
        public FsmGraph(in List<(FsmState<Id, SharedData>, Id[])> adjacencyList)
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

        public bool HasState(in Id id) =>
            _stateIdCache.TryGetIndex(id, out _);

        public bool HasTransition(in Id source, in Id dest) =>
            _stateIdCache.TryGetIndex(source, out int sourceIndex) &&
            _stateIdCache.TryGetIndex(dest,   out int destIndex)   &&
            _nodes[sourceIndex].neighbors.IsSet(destIndex);

        public FsmState<Id, SharedData> GetState(in Id id) =>
            _stateIdCache.TryGetIndex(id, out int index)? _nodes[index].state : null;


        private static Node[] ExtractNodeForEachDefinedId(
            in List<(FsmState<Id, SharedData>, Id[])> adjacencyList)
        {
            if (adjacencyList.Count != _stateIdCache.Count)
            {
                throw new ArgumentException($"Cannot extract nodes -" +
                    $"must have one state per stateId enum member yet counts are unequal");
            }

            // fill the ordered buckets according to their underlying ordinal type
            Node[] nodes = new Node[_stateIdCache.Count];
            foreach ((FsmState<Id, SharedData> state, Id[] adjacents) in adjacencyList)
            {
                if (state == null || adjacents == null)
                {
                    throw new ArgumentException($"Cannot add node - expected non null adjacency list entry");
                }

                Id sourceId = state.Id;
                if (!_stateIdCache.TryGetIndex(state.Id, out int sourceIndex))
                {
                    throw new ArgumentException($"Cannot add node - {state.Id} is not a defined {typeof(Id)} enum");
                }

                BitSet neighbors = new(_stateIdCache.Count);
                for (int i = 0; i < adjacents.Length; i++)
                {
                    Id neighborId = adjacents[i];
                    if (!_stateIdCache.TryGetIndex(neighborId, out int neighborIndex))
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} -" +
                            $"destination is not a defined {typeof(Id)} enum");
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
                foreach ((int index, string name, Id _) in _stateIdCache.Fields())
                {
                    if (node.neighbors.IsSet(index))
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

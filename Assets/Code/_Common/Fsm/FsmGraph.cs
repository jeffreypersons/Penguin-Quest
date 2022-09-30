using System;
using System.Text;
using System.Collections.Generic;
using PQ.Common.Extensions;
using System.Diagnostics.Contracts;


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
        #region cache
        // since enums are evaluated at compile time and bound to corresponding template parameter,
        // we only need to validate once, when this file first loads
        private static BitSet _stateIdEntries;
        private static BitSet CachedStateIdEntries
        {
            get
            {
                if (!EnumExtensions.AreAllEnumValuesDefault<StateId>())
                {
                    throw new ArgumentException(
                        $"Id {typeof(StateId)} underlying enum type must be int32 with default values from 0 to n - " +
                        $"received {EnumExtensions.AsUserFriendlyString<StateId>()} instead");
                }

                _stateIdEntries = new BitSet(size: EnumExtensions.CountEnumValues<StateId>());
                _stateIdEntries.SetAll();
                return _stateIdEntries;
            }
        }

        [Pure]
        private static bool TryMapIdToIndex(StateId id, out int index)
        {
            index = EnumExtensions.AsInt(id);
            if (!CachedStateIdEntries.IsSet(index))
            {
                index = -1;
                return false;
            }
            return true;
        }
        #endregion cache


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
        private static readonly string indentation = new(' ', 4);


        /* Fill the graph with states, initialize them, and add their neighbors. */
        public FsmGraph(in List<(FsmState<StateId, SharedData>, StateId[])> adjacencyList)
        {
            if (adjacencyList == null || adjacencyList.Count == 0)
            {
                throw new ArgumentException($"Fsm must have at least one state - received none");
            }

            _stateCount      = 0;
            _transitionCount = 0;
            _description     = string.Empty;
            _nodes           = ExtractNodeForEachDefinedId(adjacencyList);

            StringBuilder stringBuilder = new("{\n");
            foreach (Node node in _nodes)
            {
                stringBuilder.Append($"{indentation}{node.state.Name}=>{{").AppendJoin(',', node.neighbors).Append($"}}");
            }
            _description = $"{{\n{stringBuilder}}}";
        }

        public bool HasState(StateId id) => TryMapIdToIndex(id, out int _);

        public bool HasTransition(StateId source, StateId dest) =>
            TryMapIdToIndex(source, out int sourceIndex) &&
            TryMapIdToIndex(dest,   out int destIndex)   &&
            _nodes[sourceIndex].neighbors.IsSet(destIndex);

        public FsmState<StateId, SharedData> GetState(StateId id) =>
            TryMapIdToIndex(id, out int index)? _nodes[index].state : null;


        private static Node[] ExtractNodeForEachDefinedId(
            in List<(FsmState<StateId, SharedData>, StateId[])> adjacencyList)
        {
            // fill the ordered buckets according to their underlying ordinal type
            // note that since we enforce uniqueness and equal counts, there is no need
            // to do any further checks
            Node[] nodes = new Node[CachedStateIdEntries.Count];
            foreach ((FsmState<StateId, SharedData> state, StateId[] adjacents) in adjacencyList)
            {
                if (state == null || adjacents == null)
                {
                    throw new ArgumentException($"Cannot add node - expected non null adjacency list entry");
                }
                if (!TryMapIdToIndex(state.Id, out int stateIndex))
                {
                    throw new ArgumentException($"Cannot add node - {state.Id} is not a defined {typeof(StateId)} enum");
                }

                StateId sourceId = state.Id;
                BitSet neighbors = new(CachedStateIdEntries.Count);
                for (int i = 0; i < adjacents.Length; i++)
                {
                    StateId neighborId = adjacents[i];
                    if (!TryMapIdToIndex(neighborId, out int neighborIndex))
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} -" +
                            $"destination is not a defined {typeof(StateId)} enum");
                    }
                    if (!neighbors.TryAdd(neighborIndex))
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} - expected unique existing key");
                    }
                    if (stateIndex == neighborIndex)
                    {
                        throw new ArgumentException($"Cannot add transition {sourceId}=>{neighborId} - must be different states");
                    }
                }
                nodes[stateIndex] = new Node(state, neighbors);
            }
            return nodes;
        }
    }
}

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
        private readonly int _nodeCount;
        private readonly int _edgeCount;
        private readonly string _description;

        private readonly BitSet _stateIds;
        private readonly BitSet[] _neighbors;
        private readonly FsmState<StateId, SharedData>[] _states;

        public int StateCount      => _nodeCount;
        public int TransitionCount => _edgeCount;

        private readonly string indent = new(' ', 4);
        
        public override string ToString() => _description;

        /* Fill the graph with states, initialize them, and add their neighbors. */
        public FsmGraph(in List<(FsmState<StateId, SharedData>, StateId[])> nodes)
        {
            int stateIdCount = EnumExtensions.CountEnumValues<StateId>();
            if (nodes == null || nodes.Count == 0)
            {
                throw new ArgumentException($"Fsm must have at least one state - received none");
            }
            if (nodes.Count != stateIdCount)
            {
                throw new ArgumentException($"Fsm must have one state per stateId enum member - counts are unequal");
            }
            if (!EnumExtensions.AreAllEnumValuesDefault<StateId>())
            {
                throw new ArgumentException("Id's underlying enum type must be int32 with default values from 0 to n");
            }


            // fill the ordered buckets according to their underlying ordinal type
            // note that since we enforce uniqueness and equal counts, there is no need
            // to do any further checks
            _nodeCount   = 0;
            _edgeCount   = 0;
            _description = string.Empty;
            _stateIds    = new BitSet(stateIdCount);
            _states      = new FsmState<StateId, SharedData>[stateIdCount];
            _neighbors   = new BitSet[stateIdCount];
            foreach ((var state, var neighbors) in nodes)
            {
                if (!TryGetIndex(state, out int stateIndex))
                {
                    throw new ArgumentException($"Cannot add state to graph - expected non state with a defined state id");
                }
                if (!_stateIds.TryAdd(stateIndex))
                {
                    throw new ArgumentException($"Cannot add state to graph - non-duplicate state");
                }
                if (neighbors == null)
                {
                    throw new ArgumentException($"Cannot add state to graph - neighbors cannot be null");
                }

                state.Initialize();
                _nodeCount++;
                _edgeCount += neighbors.Length;
                _states[stateIndex] = state;
                _neighbors[stateIndex] = new BitSet(stateIdCount);
            }

            // loop through again after it's all initialized..
            StringBuilder stringBuilder = new("{\n");
            foreach ((var state, var neighbors) in nodes)
            {
                int stateIndex = EnumExtensions.AsInt(state.Id);
                foreach (StateId neighborId in neighbors)
                {
                    int neighborIndex = EnumExtensions.AsInt(neighborId);
                    if (!_stateIds.IsSet(neighborIndex) || !_neighbors[stateIndex].TryAdd(neighborIndex))
                    {
                        throw new ArgumentException($"Cannot add transition {state.Id}=>{neighborId} to graph - expected unique existing key");
                    }
                    if (FsmState<StateId, SharedData>.HasSameId(state.Id, neighborId))
                    {
                        throw new ArgumentException($"Cannot add transition {state.Id}=>{neighborId} to graph - must be different states");
                    }
                }
                stringBuilder.Append($"{indent}{state.Name}=>{{").AppendJoin(',', neighbors);
            }
            _description = $"{{\n{stringBuilder}}}";
        }

        public bool HasState(StateId id) =>
            TryGetIndex(id, out int index) &&
            _stateIds.IsSet(index);

        public bool HasTransition(StateId source, StateId dest) =>
            TryGetIndex(source, out int sourceIndex) &&
            TryGetIndex(dest,   out int destIndex)   &&
            _neighbors[sourceIndex].IsSet(destIndex);

        public FsmState<StateId, SharedData> GetState(StateId id) =>
            TryGetIndex(id, out int index)? _states[index] : null;


        private bool TryGetIndex(StateId id, out int index)
        {
            index = EnumExtensions.AsInt(id);
            if (index < 0 || index >= _states.Length)
            {
                index = -1;
                return false;
            }
            return true;
        }

        private bool TryGetIndex(FsmState<StateId, SharedData> state, out int index)
        {
            if (state == null)
            {
                index = -1;
                return false;
            }
            int enumValue = EnumExtensions.AsInt(state.Id);
            if (enumValue < 0 || enumValue >= _states.Length)
            {
                index = -1;
                return false;
            }

            index = enumValue;
            return true;
        }
    }
}

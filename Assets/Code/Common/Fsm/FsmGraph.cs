using System;
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
        private static readonly string _indentation = new(' ', 4);
        private struct Node
        {
            public readonly FsmState<StateId, SharedData> state;
            public readonly EnumSet<StateId> neighbors;
            public Node(FsmState<StateId, SharedData> state, EnumSet<StateId> neighbors)
            {
                this.state = state;
                this.neighbors = neighbors;
            }
            public override string ToString() => $"{state.Id} => {{ {string.Join(", ", neighbors.Entries)} }}";
        }

        public int StateCount => _stateCount;
        public int TransitionCount => _transitionCount;
        public override string ToString() => _description;

        private int    _stateCount;
        private int    _transitionCount;
        private string _description;
        private EnumMap<StateId, Node> _nodes;

        public FsmGraph(in List<(FsmState<StateId, SharedData> state, StateId[] adjacents)> adjacencyList)
        {
            if (adjacencyList == null || adjacencyList.Count == 0)
            {
                throw new ArgumentException($"Fsm must have at least one state - received none");
            }

            _stateCount      = 0;
            _transitionCount = 0;
            _nodes           = new EnumMap<StateId, Node>();
            foreach ((FsmState<StateId, SharedData> state, StateId[] adjacents) in adjacencyList)
            {
                AddNode(state, adjacents);
            }

            // note that we build the graph description after rather then in the loop with a string builder
            // such that everything is processed inline with map's intrinsic sorted order
            _description = $"FsmGraph{string.Join($"\n{_indentation}", _nodes.Values)}";
        }

        public bool HasTransition(StateId id, in StateId dest) =>
            _nodes.TryGetValue(id, out Node node) && node.neighbors.Contains(dest);

        public FsmState<StateId, SharedData> GetState(StateId id)
        {
            if (!_nodes.TryGetValue(id, out Node node))
            {
                throw new ArgumentException($"Given id {id} is invalid - expected a defined {typeof(StateId)}");
            }
            return node.state;
        }
        
        
        private void AddNode(FsmState<StateId, SharedData> state, StateId[] adjacents)
        {
            if (state == null || adjacents == null)
            {
                throw new ArgumentException($"Cannot add node - expected non null adjacency list entry");
            }

            var id = state.Id;
            var neighbors = new EnumSet<StateId>(adjacents);
            if (!_nodes.TryAdd(id, new Node(state, neighbors)))
            {
                throw new ArgumentException($"Cannot add node - {id} is not a defined {typeof(StateId)} enum");
            }
            if (neighbors.Contains(id))
            {
                throw new ArgumentException($"Cannot add transition {id}=>{id} - must be different states");
            }

            _stateCount++;
            _transitionCount += neighbors.Count;
        }
    }
}

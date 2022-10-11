using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using PQ.Common.Containers;


namespace PQ.Common.Fsm
{
    /*
    Layer on top of generic enum with caching and validation, done statically only once per generic enum type.
    
    Properties
    - restricts fsm id's to a maximum of 64 and minimum of 1 enum member(s) such that it can be used with bit masks
    - enforced singleton access such that validation is done only the first time the given enum is 'seen'
    - constant time 'contains' check
    - generic (no boxing!) enum comparisons via comparer (relevant since == cannot be used with generic enum types)
    - upfront validation of enum constraints (that the values follow the pattern of 0,1,2,....,n-1,n)
    */
    internal class FsmStateIdCache<TEnum>
        where TEnum : struct, Enum
    {
        // since enums are evaluated at compile time and bound to corresponding template parameter,
        // we only need to validate once, when this instance is first used
        //
        // note that if we ever need to support multi-threading, this can be made thread safe with a performance
        // penalty via C# 7's Lazy feature
        private static FsmStateIdCache<TEnum> _instance;
        public static FsmStateIdCache<TEnum> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FsmStateIdCache<TEnum>();
                }
                return _instance;
            }
        }

        private readonly OrderedEnumSet<TEnum> _enumSet;
        private FsmStateIdCache()
        {
            _enumSet = new OrderedEnumSet<TEnum>();

            if (_enumSet.Count < 1 || _enumSet.Count > 64)
            {
                throw new ArgumentException($"Given enum type must be within range [1, 64] to support bit-masking");
            }
        }

        public override string ToString() => $"{GetType()}{_enumSet}";

        public int Count => _enumSet.Count;

        public Comparer<TEnum>         ValueComparer    => _enumSet.ValueComparer;
        public EqualityComparer<TEnum> EqualityComparer => _enumSet.EqualityComparer;

        public IEnumerable<(int index, string name, TEnum id)> Fields() => _enumSet.Fields();

        [Pure] public bool TryGetIndex(TEnum id, out int index) => _enumSet.TryGetIndex(id, out index);

        [Pure] public string GetName(in TEnum id) => _enumSet.GetName(id);
    }
}

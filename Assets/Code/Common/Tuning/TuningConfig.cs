using System;
using UnityEngine;


namespace PQ.Common.Tuning
{
    /*
    Collection of tuning data, with a listener built in.
    */
    public abstract class TuningConfig : ScriptableObject
    {
        private event Action _onChanged = delegate { };

        public override string ToString() => $"{GetType()}";

        public void RegisterOnChanged(Action onChanged) => _onChanged += onChanged;

        void OnValidate() => _onChanged.Invoke();
    }
}

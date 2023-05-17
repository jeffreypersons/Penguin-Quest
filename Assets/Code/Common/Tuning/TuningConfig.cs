using UnityEngine;
using PQ.Common.Events;


namespace PQ.Common.Tuning
{
    /*
    Collection of tuning data, with a listener built in.

    Intentionally makes zero assumptions about the data that may be contained in implementing classes.
    */
    // todo: figure out how to make this work with inheritance
    //[CreateAssetMenu(fileName = "TuningConfig",menuName = "TuningConfigs/TuningConfig",order = 1)]
    public abstract class TuningConfig : ScriptableObject
    {
        private static readonly string _configName = typeof(TuningConfig).Name;

        private PqEvent _onChanged = new($"{_configName}.changed");

        private void OnValidate() => _onChanged.Raise();


        public override string ToString() => $"{_configName}";

        public IPqEventReceiver OnChanged => _onChanged;

    }
}

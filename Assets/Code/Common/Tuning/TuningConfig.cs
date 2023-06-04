using System;
using System.Collections;
using UnityEngine;
using PQ.Common.Events;


namespace PQ.Common.Tuning
{
    /*
    Collection of tuning data, with a listener built in.

    Intentionally makes zero assumptions about the data that may be contained in implementing classes.
    Also serves as a unified interface that hides the many gotchas that arise from dealing directly with
    scriptable objects.
    */
    // todo: figure out how to make this work with inheritance
    //[CreateAssetMenu(fileName="TuningConfig", menuName="TuningConfigs/TuningConfig", order=1)]
    public abstract class TuningConfig : ScriptableObject
    {
        private static readonly string _configName = typeof(TuningConfig).Name;

        [NonSerialized] private PqEvent _onChanged = new($"{_configName}.changed");


        public IPqEventReceiver OnChangedInEditor => _onChanged;

        public override string ToString()
        {
            return $"{_configName}";
        }


        // todo: add support for onChanged even if not running in editor (eg a debug menu)
        void OnValidate()
        {
            // queue the onChanged event to fire only at the end of the frame such that any monobehaviors
            // listening for the event have a chance to have any initialization code run _first_ in
            // their OnValidate/Awake callbacks (eg for an entity class that has an [ExecuteAlways] attribute)
            RaiseOnEndOfFrame(_onChanged);
        }


        private static IEnumerator RaiseOnEndOfFrame(PqEvent eventToRaise)
        {
            yield return new WaitForEndOfFrame();
            eventToRaise.Raise();
        }
    }
}

#if false
using UnityEditor;
using UnityEngine;

namespace Crosstales.TPS.Example
{
#if UNITY_EDITOR
    /// <summary>Example editor integration of Turbo Switch for your own scripts.</summary>
    public static class TPSMenu
    {
        [MenuItem("Tools/Switch to Windows #&w")]
        public static void SwitchWindows()
        {
            Debug.Log("Switch to Windows");

            Switcher.Switch(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Tools/Switch to Android #&m")]
        public static void SwitchAndroid()
        {
            Debug.Log("Switch to Android");

            Switcher.Switch(BuildTarget.Android);
        }
    }
}
#endif
#endif
// © 2019 crosstales LLC (https://www.crosstales.com)
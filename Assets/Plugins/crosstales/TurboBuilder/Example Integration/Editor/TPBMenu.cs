#if false
using UnityEditor;
using UnityEngine;

namespace Crosstales.TPB.Example
{
#if UNITY_EDITOR
    /// <summary>Example editor integration of Turbo Builder for your own scripts.</summary>
    public static class TPBMenu
    {
        [MenuItem("Tools/Build Windows #&w")]
        public static void BuildWindows()
        {
            Debug.Log("Build Windows");

            Builder.Build(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Tools/Build Android #&m")]
        public static void BuildAndroid()
        {
            Debug.Log("Build Android");

            Builder.Build(BuildTarget.Android);
        }

        [MenuItem("Tools/Build All")]
        public static void BuildAll()
        {
            Debug.Log("Build All");

            Builder.BuildAll();
        }

    }
}
#endif
#endif
// © 2019-2020 crosstales LLC (https://www.crosstales.com)
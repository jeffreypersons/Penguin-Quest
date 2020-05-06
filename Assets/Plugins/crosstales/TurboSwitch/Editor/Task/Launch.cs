#if UNITY_EDITOR
using UnityEditor;

namespace Crosstales.TPS.Task
{
   /// <summary>Show the configuration window on the first launch.</summary>
   [InitializeOnLoad]
   public static class Launch
   {
      #region Constructor

      static Launch()
      {
         bool launched = EditorPrefs.GetBool(Util.Constants.KEY_LAUNCH);

         if (!launched)
         {
            EditorIntegration.ConfigWindow.ShowWindow(0);
            EditorPrefs.SetBool(Util.Constants.KEY_LAUNCH, true);
         }
      }

      #endregion
   }
}
#endif
// © 2017-2020 crosstales LLC (https://www.crosstales.com)
#if UNITY_EDITOR
using UnityEditor;
using Crosstales.TPB.Util;

namespace Crosstales.TPB.Task
{
   /// <summary>Show the configuration window on the first launch.</summary>
   [InitializeOnLoad]
   public static class Launch
   {
      #region Constructor

      static Launch()
      {
         bool launched = EditorPrefs.GetBool(Constants.KEY_LAUNCH);

         if (!launched)
         {
            EditorIntegration.ConfigWindow.ShowWindow(0);
            EditorPrefs.SetBool(Constants.KEY_LAUNCH, true);
         }
      }

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
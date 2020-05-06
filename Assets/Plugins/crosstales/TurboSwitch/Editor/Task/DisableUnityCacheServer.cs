#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.TPS.Task
{
   /// <summary>Disables the Unity cache server.</summary>
   [InitializeOnLoad]
   public static class DisableUnityCacheServer
   {
      #region Constructor

      static DisableUnityCacheServer()
      {
         if (EditorPrefs.GetInt("CacheServerMode") != 2)
         {
            EditorPrefs.SetInt("CacheServerMode", 2);

            Debug.LogWarning(Common.Util.BaseHelper.CreateString("-", 400));
            Debug.LogWarning("<b>+++ 'Unity Cache Server' has been disabled for <color=blue>" + Util.Constants.ASSET_NAME + "</color>! +++</b>");
            Debug.LogWarning(Common.Util.BaseHelper.CreateString("-", 400));
         }
      }

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
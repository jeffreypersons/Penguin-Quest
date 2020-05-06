#if UNITY_EDITOR
using UnityEditor;

namespace Crosstales.TPS.Task
{
   /// <summary>Setup Unity after a switch.</summary>
   [InitializeOnLoad]
   public static class SetupUnity
   {
      #region Constructor

      static SetupUnity()
      {
         if (Util.Config.USE_LEGACY && Util.Config.SETUP_DATE < Util.Config.SWITCH_DATE)
         {
            Util.Helper.SetAndroidTexture();
            Util.Helper.RefreshAssetDatabase();

            Util.Config.SETUP_DATE = System.DateTime.Now;
            Util.Config.Save();
         }
      }

      #endregion
   }
}
#endif
// © 2019 crosstales LLC (https://www.crosstales.com)
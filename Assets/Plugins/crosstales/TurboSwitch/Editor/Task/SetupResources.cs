#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Crosstales.TPS.Task
{
   /// <summary>Copies all resources to 'Editor Default Resources'.</summary>
   [InitializeOnLoad]
   public abstract class SetupResources : Common.EditorTask.BaseSetupResources
   {
      #region Constructor

      static SetupResources()
      {
#if !CT_DEVELOP
            string path = Application.dataPath;
            string assetpath = "Assets" + Util.Config.ASSET_PATH;

            string sourceFolder = path + Util.Config.ASSET_PATH + "Icons/";
            string source = assetpath + "Icons/";

            string targetFolder = path + "/Editor Default Resources/crosstales/TurboSwitch/";
            string target = "Assets/Editor Default Resources/crosstales/TurboSwitch/";
            string metafile = assetpath + "Icons.meta";

            setupResources(source, sourceFolder, target, targetFolder, metafile);
#endif
      }

      #endregion
   }
}
#endif
// © 2016-2020 crosstales LLC (https://www.crosstales.com)
#if UNITY_EDITOR && !UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace Crosstales.TPS.EditorIntegration
{
   /// <summary>Unity "Preferences" extension.</summary>
   public class ConfigPreferences : ConfigBase
   {
      #region Variables

      private static int tab = 0;
      private static int lastTab = 0;
      private static ConfigPreferences cp;

      #endregion


      #region Static methods

      [PreferenceItem(Util.Constants.ASSET_NAME_SHORT)]
      private static void PreferencesGUI()
      {
         if (cp == null)
         {
            cp = CreateInstance(typeof(ConfigPreferences)) as ConfigPreferences;
         }

         init();

         tab = GUILayout.Toolbar(tab, new[] {"Switch", "Config", "Help", "About"});

         if (tab != lastTab)
         {
            lastTab = tab;
            GUI.FocusControl(null);
         }

         switch (tab)
         {
            case 0:
               cp.showSwitch();
               break;
            case 1:
            {
               cp.showConfiguration();

               Util.Helper.SeparatorUI();

               if (GUILayout.Button(new GUIContent(" Reset", Util.Helper.Icon_Reset, "Resets the configuration settings for this project.")))
               {
                  if (EditorUtility.DisplayDialog("Reset configuration?", "Reset the configuration of " + Util.Constants.ASSET_NAME + "?", "Yes", "No"))
                  {
                     Util.Config.Reset();
                     save();
                  }
               }

               GUILayout.Space(6);
               break;
            }
            case 2:
               cp.showHelp();
               break;
            default:
               cp.showAbout();
               break;
         }

         if (GUI.changed)
         {
            save();
         }
      }

      #endregion
   }
}
#endif
// © 2016-2020 crosstales LLC (https://www.crosstales.com)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Crosstales.TPB.Util;

namespace Crosstales.TPB.EditorIntegration
{
   /// <summary>Editor window extension.</summary>
   [InitializeOnLoad]
   public class ConfigWindow : ConfigBase
   {
      #region Variables

      private int tab = 0;
      private int lastTab = 0;

      #endregion


      #region EditorWindow methods

      [MenuItem("Window/" + Constants.ASSET_NAME, false, 1010)]
      public static void ShowWindow()
      {
         EditorWindow.GetWindow(typeof(ConfigWindow));
      }

      public static void ShowWindow(int tab)
      {
         ConfigWindow window = EditorWindow.GetWindow(typeof(ConfigWindow)) as ConfigWindow;
         if (window != null) window.tab = tab;
      }

      public void OnEnable()
      {
         titleContent = new GUIContent(Constants.ASSET_NAME_SHORT, Helper.Logo_Asset_Small);

         init();
      }

      public void OnDestroy()
      {
         save();
      }

      public void OnLostFocus()
      {
         save();
      }

      public void OnGUI()
      {
         tab = GUILayout.Toolbar(tab, new[] {"Build", "Config", "Help", "About"});

         if (tab != lastTab)
         {
            lastTab = tab;
            GUI.FocusControl(null);
         }

         if (tab == 0)
         {
            showBuild();
         }
         else if (tab == 1)
         {
            showConfiguration();

            Helper.SeparatorUI();

            GUILayout.BeginHorizontal();
            {
               if (GUILayout.Button(new GUIContent(" Save", Helper.Icon_Save, "Saves the configuration settings for this project.")))
               {
                  save();
               }

               if (GUILayout.Button(new GUIContent(" Reset", Helper.Icon_Reset, "Resets the configuration settings for this project.")))
               {
                  if (EditorUtility.DisplayDialog("Reset configuration?", "Reset the configuration of " + Constants.ASSET_NAME + "?", "Yes", "No"))
                  {
                     Config.Reset();
                     save();
                  }
               }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
         }
         else if (tab == 2)
         {
            showHelp();
         }
         else
         {
            showAbout();
         }
      }

      public void OnInspectorUpdate()
      {
         Repaint();
      }

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
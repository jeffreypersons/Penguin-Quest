#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Crosstales.TPS.Util;
using Crosstales.TPS.Task;

namespace Crosstales.TPS.EditorIntegration
{
   /// <summary>Base class for editor windows.</summary>
   public abstract class ConfigBase : EditorWindow
   {
      #region Variables

      private static string updateText = UpdateCheck.TEXT_NOT_CHECKED;
      private static UpdateStatus updateStatus = UpdateStatus.NOT_CHECKED;

      private System.Threading.Thread worker;

      private Vector2 scrollPosSwitch;
      private Vector2 scrollPosPlatforms;
      private Vector2 scrollPosConfig;
      private Vector2 scrollPosHelp;
      private Vector2 scrollPosAboutUpdate;
      private Vector2 scrollPosAboutReadme;
      private Vector2 scrollPosAboutVersions;

      private static string readme;
      private static string versions;

      private int aboutTab = 0;

      private const int rowWidth = 10000;
      private static int rowHeight = 0; //later = 36
      private static int rowCounter = 0;

      private const int logoWidth = 36;
      private const int platformWidth = 128;
      private const int architectureWidth = 64;
      private const int textureWidth = 72;
      private const int cacheWidth = 40;
      private const int actionWidth = 48;

      private static int platformX = 0;
      private static int platformY = 0;

      private static int platformTextSpace = 12; //later = 18
      private static int cacheTextSpace = 6; //later = 18
      private static int actionTextSpace = 6; //later = 8
      private const int space = 4;

      private static readonly string[] vcsOptions = {"None", "git", "SVN", "Mercurial", "Collab", "PlasticSCM"};

      private static readonly string[] archWinOptions = {"32bit", "64bit"};
#if !UNITY_2019_2_OR_NEWER
      private static readonly string[] archLinuxOptions = {"32bit", "64bit", "Universal"};
#endif
#if UNITY_5 || UNITY_2017
      private static readonly string[] texAndroidOptions = {"Generic", "DXT", "PVRTC", "ATC", "ETC", "ETC2", "ASTC"};
#else
        private static readonly string[] texAndroidOptions = {"Generic", "DXT", "PVRTC", "ETC", "ETC2", "ASTC"};
#endif
      private static bool platformWindows;
      private static bool platformMac;
      private static bool platformLinux;
      private static bool platformAndroid;
      private static bool platformIOS;
      private static bool platformWSA;
      private static bool platformWebGL;
      private static bool platformTvOS;
      private static bool platformPS4;
      private static bool platformXboxOne;
      private static bool platformSwitch;
#if !UNITY_2018_2_OR_NEWER
      private static bool platformPSP2;
      private static bool platformWiiU;
      private static bool platform3DS;
#endif

      #endregion


      #region Properties

      private static BuildTarget targetWindows
      {
         get { return Config.ARCH_WINDOWS == 0 ? BuildTarget.StandaloneWindows : BuildTarget.StandaloneWindows64; }
      }

      private static BuildTarget targetMac
      {
         get { return BuildTarget.StandaloneOSX; }
      }

      private static BuildTarget targetLinux
      {
         get
         {
#if UNITY_2019_2_OR_NEWER
                return BuildTarget.StandaloneLinux64;
#else
            if (Config.ARCH_LINUX == 0)
            {
               return BuildTarget.StandaloneLinux;
            }

            return Config.ARCH_LINUX == 1 ? BuildTarget.StandaloneLinux64 : BuildTarget.StandaloneLinuxUniversal;

#endif
         }
      }

#if UNITY_5 || UNITY_2017
      private static MobileTextureSubtarget texAndroid
      {
         get
         {
            switch (Config.TEX_ANDROID)
            {
               case 1:
                  return MobileTextureSubtarget.DXT;
               case 2:
                  return MobileTextureSubtarget.PVRTC;
               case 3:
                  return MobileTextureSubtarget.ATC;
               case 4:
                  return MobileTextureSubtarget.ETC;
               case 5:
                  return MobileTextureSubtarget.ETC2;
               case 6:
                  return MobileTextureSubtarget.ASTC;
               default:
                  return MobileTextureSubtarget.Generic;
            }
         }
      }
#else
        private static MobileTextureSubtarget texAndroid
        {
            get
            {
                switch (Config.TEX_ANDROID)
                {
                    case 1:
                        return MobileTextureSubtarget.DXT;
                    case 2:
                        return MobileTextureSubtarget.PVRTC;
                    case 3:
                        return MobileTextureSubtarget.ETC;
                    case 4:
                        return MobileTextureSubtarget.ETC2;
                    case 5:
                        return MobileTextureSubtarget.ASTC;
                    default:
                        return MobileTextureSubtarget.Generic;
                }
            }
        }
#endif

      #endregion


      #region Protected methods

      protected static void init()
      {
         platformWindows = Helper.isValidBuildTarget(BuildTarget.StandaloneWindows) || Helper.isValidBuildTarget(BuildTarget.StandaloneWindows64);
         platformMac = Helper.isValidBuildTarget(BuildTarget.StandaloneOSX);
#if UNITY_2019_2_OR_NEWER
         platformLinux = Helper.isValidBuildTarget(BuildTarget.StandaloneLinux64);
#else
         platformLinux = Helper.isValidBuildTarget(BuildTarget.StandaloneLinux) || Helper.isValidBuildTarget(BuildTarget.StandaloneLinux64) || Helper.isValidBuildTarget(BuildTarget.StandaloneLinuxUniversal);
#endif
         platformAndroid = Helper.isValidBuildTarget(BuildTarget.Android);
         platformIOS = Helper.isValidBuildTarget(BuildTarget.iOS);
         platformWSA = Helper.isValidBuildTarget(BuildTarget.WSAPlayer);
         platformWebGL = Helper.isValidBuildTarget(BuildTarget.WebGL);
         platformTvOS = Helper.isValidBuildTarget(BuildTarget.tvOS);
         platformPS4 = Helper.isValidBuildTarget(BuildTarget.PS4);
         platformXboxOne = Helper.isValidBuildTarget(BuildTarget.XboxOne);
         platformSwitch = Helper.isValidBuildTarget(BuildTarget.Switch);
#if !UNITY_2018_2_OR_NEWER
         platformPSP2 = Helper.isValidBuildTarget(BuildTarget.PSP2);
         platformWiiU = Helper.isValidBuildTarget(BuildTarget.WiiU);
         platform3DS = Helper.isValidBuildTarget(BuildTarget.N3DS);
#endif
      }

      protected void showSwitch()
      {
         /*
         if (Context.isSwitchInProgress) {
             EditorUtility.DisplayProgressBar("Switching to " + Context.CurrentTarget, Context.CurrentTask, Context.SwitchProgress);

             if (Context.isSwitchComplete)
             {
                 Helper.EndSwitchPlatform();
             }
         }
         */

         GUI.skin.label.wordWrap = true;

         if (Helper.isEditorMode)
         {
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
               if (Helper.hasActivePlatforms)
               {
                  platformX = 0;
                  platformY = 0;
                  platformTextSpace = 12; //later = 18

                  rowHeight = 0; //later = 36
                  rowCounter = 0;

                  cacheTextSpace = 6; //later = 18
                  actionTextSpace = 6; //later = 8

                  // header
                  drawHeader();

                  scrollPosSwitch = EditorGUILayout.BeginScrollView(scrollPosSwitch, false, false);
                  {
                     //content
                     drawContent();
                  }
                  EditorGUILayout.EndScrollView();

                  if (Helper.hasCache)
                  {
                     Helper.SeparatorUI();

                     Config.SHOW_DELETE = EditorGUILayout.Toggle(new GUIContent("Show Delete Buttons", "Shows or hides the delete button for the cache."), Config.SHOW_DELETE);

                     GUILayout.Space(6);
                  }
               }
               else
               {
                  EditorGUILayout.HelpBox("Please enable the desired platforms under 'Config'!", MessageType.Warning);
               }
            }
            else
            {
               EditorGUILayout.HelpBox("Unity Editor is busy, please wait...", MessageType.Info);
            }
         }
         else
         {
            EditorGUILayout.HelpBox("Disabled in Play-mode!", MessageType.Info);
         }
      }

      protected void showConfiguration()
      {
         tpbBanner();

         scrollPosPlatforms = EditorGUILayout.BeginScrollView(scrollPosPlatforms, false, false);
         {
            GUILayout.Label("General Settings", EditorStyles.boldLabel);
            Config.CUSTOM_PATH_CACHE = EditorGUILayout.BeginToggleGroup(new GUIContent("Custom Cache Path", "Enable or disable a custom cache path (default: " + Constants.DEFAULT_CUSTOM_PATH_CACHE + ")."), Config.CUSTOM_PATH_CACHE);
            {
               EditorGUI.indentLevel++;

               EditorGUILayout.BeginHorizontal();
               {
                  EditorGUILayout.SelectableLabel(Config.PATH_CACHE);

                  if (GUILayout.Button(new GUIContent(" Select", Helper.Icon_Folder, "Select path for the cache")))
                  {
                     string path = EditorUtility.OpenFolderPanel("Select path for the cache", Config.PATH_CACHE.Substring(0, Config.PATH_CACHE.Length - (Constants.CACHE_DIRNAME.Length + 1)), string.Empty);

                     if (!string.IsNullOrEmpty(path))
                     {
                        Config.PATH_CACHE = path + "/" + Constants.CACHE_DIRNAME;
                     }
                  }
               }
               EditorGUILayout.EndHorizontal();

               EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndToggleGroup();

            //GUILayout.Space(space);
            GUI.enabled = !Config.CUSTOM_PATH_CACHE;

            Config.VCS = EditorGUILayout.Popup("Version Control", Config.VCS, vcsOptions);

            GUILayout.Space(space);
            GUI.enabled = true;

            Config.USE_LEGACY = EditorGUILayout.BeginToggleGroup(new GUIContent("Legacy Mode", "Enable or disable legacy mode. If enabled, switch will close and restart Unity (default: " + Constants.DEFAULT_USE_LEGACY + ")."), Config.USE_LEGACY);
            {
               EditorGUI.indentLevel++;

               Config.DELETE_LOCKFILE = EditorGUILayout.Toggle(new GUIContent("Delete UnityLockfile", "Enable or disable deleting the 'UnityLockfile' (default: " + Constants.DEFAULT_DELETE_LOCKFILE + ")."), Config.DELETE_LOCKFILE);

               EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndToggleGroup();

            GUILayout.Space(space);

            Config.BATCHMODE = EditorGUILayout.BeginToggleGroup(new GUIContent("Batch Mode", "Enable or disable batch mode for CLI operations (default: " + Constants.DEFAULT_BATCHMODE + ")"), Config.BATCHMODE);
            {
               EditorGUI.indentLevel++;

               Config.QUIT = EditorGUILayout.Toggle(new GUIContent("Quit", "Enable or disable quit Unity Editor for CLI operations (default: " + Constants.DEFAULT_QUIT + ")."), Config.QUIT);

               Config.NO_GRAPHICS = EditorGUILayout.Toggle(new GUIContent("No Graphics", "Enable or disable graphics device in Unity Editor for CLI operations (default: " + Constants.DEFAULT_NO_GRAPHICS + ")."), Config.NO_GRAPHICS);

               EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndToggleGroup();

            GUILayout.Space(space);

            Config.DEBUG = EditorGUILayout.Toggle(new GUIContent("Debug", "Enable or disable debug logs (default: " + Constants.DEFAULT_DEBUG + ")."), Config.DEBUG);

            Config.UPDATE_CHECK = EditorGUILayout.Toggle(new GUIContent("Update Check", "Enable or disable the update-checks for the asset (default: " + Constants.DEFAULT_UPDATE_CHECK + ")"), Config.UPDATE_CHECK);

            Config.COMPILE_DEFINES = EditorGUILayout.Toggle(new GUIContent("Compile Defines", "Enable or disable adding compile define 'CT_TPS' for the asset (default: " + Constants.DEFAULT_COMPILE_DEFINES + ")"), Config.COMPILE_DEFINES);

            //Config.REMINDER_CHECK = EditorGUILayout.Toggle(new GUIContent("Reminder Check", "Enable or disable the reminder-check (default: " + Constants.DEFAULT_REMINDER_CHECK + ")"), Config.REMINDER_CHECK);

            Helper.SeparatorUI();

            GUILayout.Label("Switch Settings", EditorStyles.boldLabel);
            Config.EXECUTE_METHOD_PRE_SWITCH = EditorGUILayout.TextField(new GUIContent("Method Before Switch", "Execute static method <ClassName.MethodName> in Unity before a switch (default: empty, e.g. 'Crosstales.TPS.Switcher.MethodBeforeSwitch')."), Config.EXECUTE_METHOD_PRE_SWITCH);
            Config.EXECUTE_METHOD = EditorGUILayout.TextField(new GUIContent("Method After Switch", "Execute static method <ClassName.MethodName> in Unity after a switch (default: empty, e.g. 'Crosstales.TPS.Switcher.MethodAfterSwitch')."), Config.EXECUTE_METHOD);

            //Config.COPY_ASSETS = EditorGUILayout.Toggle(new GUIContent("Copy Assets", "Enable or disable the copying the 'Assets' folder (default: " + Constants.DEFAULT_COPY_ASSETS + ")."), Config.COPY_ASSETS);
            //Config.COPY_LIBRARY = EditorGUILayout.Toggle(new GUIContent("Copy Library", "Enable or disable the copying the 'Library' folder (default: " + Constants.DEFAULT_COPY_LIBRARY + ")."), Config.COPY_LIBRARY);
            Config.COPY_SETTINGS = EditorGUILayout.Toggle(new GUIContent("Copy ProjectSettings", "Enable or disable copying the 'ProjectSettings'-folder (default: " + Constants.DEFAULT_COPY_SETTINGS + ")."), Config.COPY_SETTINGS);

            //Config.COPY_ASSETS = EditorGUILayout.Toggle(new GUIContent("Copy Assets", "Enable or disable copying the 'Assets'-folder (default: " + Constants.DEFAULT_COPY_ASSETS + ")."), Config.COPY_ASSETS);

            Helper.SeparatorUI();

            GUILayout.Label("Active Platforms", EditorStyles.boldLabel);

            if (platformWindows)
               Config.PLATFORM_WINDOWS = EditorGUILayout.Toggle(new GUIContent("Windows", "Enable or disable the support for the Windows platform."), Config.PLATFORM_WINDOWS);

            if (platformMac)
               Config.PLATFORM_MAC = EditorGUILayout.Toggle(new GUIContent("macOS", "Enable or disable the support for the macOS platform."), Config.PLATFORM_MAC);

            if (platformLinux)
               Config.PLATFORM_LINUX = EditorGUILayout.Toggle(new GUIContent("Linux", "Enable or disable the support for the Linux platform."), Config.PLATFORM_LINUX);

            if (platformAndroid)
               Config.PLATFORM_ANDROID = EditorGUILayout.Toggle(new GUIContent("Android", "Enable or disable the support for the Android platform."), Config.PLATFORM_ANDROID);

            if (platformIOS)
               Config.PLATFORM_IOS = EditorGUILayout.Toggle(new GUIContent("iOS", "Enable or disable the support for the iOS platform."), Config.PLATFORM_IOS);

            if (platformWSA)
               Config.PLATFORM_WSA = EditorGUILayout.Toggle(new GUIContent("UWP (WSA)", "Enable or disable the support for the UWP (WSA) platform."), Config.PLATFORM_WSA);

            if (platformWebGL)
               Config.PLATFORM_WEBGL = EditorGUILayout.Toggle(new GUIContent("WebGL", "Enable or disable the support for the WebGL platform."), Config.PLATFORM_WEBGL);

            if (platformTvOS)
               Config.PLATFORM_TVOS = EditorGUILayout.Toggle(new GUIContent("tvOS", "Enable or disable the support for the tvOS platform."), Config.PLATFORM_TVOS);

            if (platformPS4)
               Config.PLATFORM_PS4 = EditorGUILayout.Toggle(new GUIContent("PS4", "Enable or disable the support for the Sony PS4 platform."), Config.PLATFORM_PS4);

            if (platformXboxOne)
               Config.PLATFORM_XBOXONE = EditorGUILayout.Toggle(new GUIContent("XBoxOne", "Enable or disable the support for the Microsoft XBoxOne platform."), Config.PLATFORM_XBOXONE);

            if (platformSwitch)
               Config.PLATFORM_SWITCH = EditorGUILayout.Toggle(new GUIContent("Switch", "Enable or disable the support for the Nintendo Switch platform."), Config.PLATFORM_SWITCH);

#if !UNITY_2018_2_OR_NEWER
            if (platformPSP2)
               Config.PLATFORM_PSP2 = EditorGUILayout.Toggle(new GUIContent("PSP2 (Vita)", "Enable or disable the support for the Sony PSP2 (Vita) platform."), Config.PLATFORM_PSP2);

            if (platformWiiU)
               Config.PLATFORM_WIIU = EditorGUILayout.Toggle(new GUIContent("WiiU", "Enable or disable the support for the Nintendo WiiU platform."), Config.PLATFORM_WIIU);

            if (platform3DS)
               Config.PLATFORM_3DS = EditorGUILayout.Toggle(new GUIContent("3DS", "Enable or disable the support for the Nintendo 3DS platform."), Config.PLATFORM_3DS);
#endif
            Helper.SeparatorUI();

            GUILayout.Label("UI Settings", EditorStyles.boldLabel);
            Config.CONFIRM_SWITCH = EditorGUILayout.Toggle(new GUIContent("Confirm Switch", "Enable or disable the switch confirmation dialog (default: " + Constants.DEFAULT_CONFIRM_SWITCH + ")."), Config.CONFIRM_SWITCH);
            Config.SHOW_COLUMN_PLATFORM = EditorGUILayout.Toggle(new GUIContent("Column: Platform", "Enable or disable the column 'Platform' in the 'Switch'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_PLATFORM + ")."), Config.SHOW_COLUMN_PLATFORM);
            Config.SHOW_COLUMN_ARCHITECTURE = EditorGUILayout.Toggle(new GUIContent("Column: Arch", "Enable or disable the column 'Arch' in the 'Switch'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_ARCHITECTURE + ")."), Config.SHOW_COLUMN_ARCHITECTURE);
            Config.SHOW_COLUMN_TEXTURE = EditorGUILayout.Toggle(new GUIContent("Column: Texture", "Enable or disable the column 'Texture' in the 'Switch'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_TEXTURE + ")."), Config.SHOW_COLUMN_TEXTURE);
            Config.SHOW_COLUMN_CACHE = EditorGUILayout.Toggle(new GUIContent("Column: Cache", "Enable or disable the column 'Cache' in the 'Switch'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_CACHE + ")."), Config.SHOW_COLUMN_CACHE);
         }
         EditorGUILayout.EndScrollView();

         Helper.SeparatorUI();

         GUILayout.Label("Cache Usage", EditorStyles.boldLabel);

         GUI.skin.label.wordWrap = true;

         GUILayout.Label(Helper.CacheInfo);

         GUI.skin.label.wordWrap = false;

         GUI.enabled = Helper.hasCache && !Helper.isDeleting;

         if (GUILayout.Button(new GUIContent(" Show Cache", Helper.Icon_Show, "Show the cache.")))
         {
            Helper.ShowFileLocation(Config.PATH_CACHE);
         }

         if (GUILayout.Button(new GUIContent(" Delete Cache", Helper.Icon_Delete, "Delete the complete cache")))
         {
            if (EditorUtility.DisplayDialog("Delete the complete cache?", "If you delete the complete cache, Unity must re-import all assets for every platform switch." + System.Environment.NewLine + "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to delete the cache?", "Yes", "No"))
            {
               if (Config.DEBUG)
                  Debug.Log("Complete cache deleted");

               Helper.DeleteCache();
            }
         }

         GUI.enabled = true;
      }

      protected void showHelp()
      {
         tpbBanner();

         scrollPosHelp = EditorGUILayout.BeginScrollView(scrollPosHelp, false, false);
         {
            GUILayout.Label("Resources", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            {
               GUILayout.BeginVertical();
               {
                  if (GUILayout.Button(new GUIContent(" Manual", Helper.Icon_Manual, "Show the manual.")))
                     Helper.OpenURL(Constants.ASSET_MANUAL_URL);

                  GUILayout.Space(6);

                  if (GUILayout.Button(new GUIContent(" Forum", Helper.Icon_Forum, "Visit the forum page.")))
                     Helper.OpenURL(Constants.ASSET_FORUM_URL);
               }
               GUILayout.EndVertical();

               GUILayout.BeginVertical();
               {
                  if (GUILayout.Button(new GUIContent(" API", Helper.Icon_API, "Show the API.")))
                     Helper.OpenURL(Constants.ASSET_API_URL);

                  GUILayout.Space(6);

                  if (GUILayout.Button(new GUIContent(" Product", Helper.Icon_Product, "Visit the product page.")))
                     Helper.OpenURL(Constants.ASSET_WEB_URL);
               }
               GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            Helper.SeparatorUI();

            GUILayout.Label("Videos", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            {
               if (GUILayout.Button(new GUIContent(" Promo", Helper.Video_Promo, "View the promotion video on 'Youtube'.")))
                  Helper.OpenURL(Constants.ASSET_VIDEO_PROMO);

               if (GUILayout.Button(new GUIContent(" Tutorial", Helper.Video_Tutorial, "View the tutorial video on 'Youtube'.")))
                  Helper.OpenURL(Constants.ASSET_VIDEO_TUTORIAL);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            if (GUILayout.Button(new GUIContent(" All Videos", Helper.Icon_Videos, "Visit our 'Youtube'-channel for more videos.")))
               Helper.OpenURL(Constants.ASSET_SOCIAL_YOUTUBE);

            Helper.SeparatorUI();

            GUILayout.Label("3rd Party Assets", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent(" RockTomate", Helper.Asset_RockTomate, "More information about 'RockTomate'.")))
               Helper.OpenURL(Util.Constants.ASSET_3P_ROCKTOMATE);
         }
         EditorGUILayout.EndScrollView();

         GUILayout.Space(6);
      }

      protected void showAbout()
      {
         tpbBanner();

         GUILayout.Space(3);
         GUILayout.Label(Constants.ASSET_NAME, EditorStyles.boldLabel);

         GUILayout.BeginHorizontal();
         {
            GUILayout.BeginVertical(GUILayout.Width(60));
            {
               GUILayout.Label("Version:");

               GUILayout.Space(12);

               GUILayout.Label("Web:");

               GUILayout.Space(2);

               GUILayout.Label("Email:");
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(170));
            {
               GUILayout.Space(0);

               GUILayout.Label(Constants.ASSET_VERSION);

               GUILayout.Space(12);

               EditorGUILayout.SelectableLabel(Constants.ASSET_AUTHOR_URL, GUILayout.Height(16), GUILayout.ExpandHeight(false));

               GUILayout.Space(2);

               EditorGUILayout.SelectableLabel(Constants.ASSET_CONTACT, GUILayout.Height(16), GUILayout.ExpandHeight(false));
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
               //GUILayout.Space(0);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(64));
            {
               if (GUILayout.Button(new GUIContent(string.Empty, Helper.Logo_Asset, "Visit asset website")))
                  Helper.OpenURL(Constants.ASSET_URL);
            }
            GUILayout.EndVertical();
         }
         GUILayout.EndHorizontal();

         GUILayout.Label("© 2016-2020 by " + Constants.ASSET_AUTHOR);

         Helper.SeparatorUI();

         GUILayout.BeginHorizontal();
         {
            if (GUILayout.Button(new GUIContent(" AssetStore", Helper.Logo_Unity, "Visit the 'Unity AssetStore' website.")))
               Helper.OpenURL(Constants.ASSET_CT_URL);

            if (GUILayout.Button(new GUIContent(" " + Constants.ASSET_AUTHOR, Helper.Logo_CT, "Visit the '" + Constants.ASSET_AUTHOR + "' website.")))
               Helper.OpenURL(Constants.ASSET_AUTHOR_URL);
         }
         GUILayout.EndHorizontal();

         Helper.SeparatorUI();

         aboutTab = GUILayout.Toolbar(aboutTab, new[] {"Readme", "Versions", "Update"});

         switch (aboutTab)
         {
            case 2:
            {
               scrollPosAboutUpdate = EditorGUILayout.BeginScrollView(scrollPosAboutUpdate, false, false);
               {
                  Color fgColor = GUI.color;

                  GUI.color = Color.yellow;

                  switch (updateStatus)
                  {
                     case UpdateStatus.NO_UPDATE:
                        GUI.color = Color.green;
                        GUILayout.Label(updateText);
                        break;
                     case UpdateStatus.UPDATE:
                     {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" Download", "Visit the 'Unity AssetStore' to download the latest version.")))
                           UnityEditorInternal.AssetStore.Open("content/" + Constants.ASSET_ID);

                        break;
                     }
                     case UpdateStatus.UPDATE_PRO:
                     {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" Upgrade", "Upgrade to the PRO-version in the 'Unity AssetStore'.")))
                           Helper.OpenURL(Constants.ASSET_PRO_URL);

                        break;
                     }
                     case UpdateStatus.UPDATE_VERSION:
                     {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" Upgrade", "Upgrade to the newer version in the 'Unity AssetStore'")))
                           Helper.OpenURL(Constants.ASSET_CT_URL);

                        break;
                     }
                     case UpdateStatus.DEPRECATED:
                     {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" More Information", "Visit the 'crosstales'-site for more information.")))
                           Helper.OpenURL(Constants.ASSET_AUTHOR_URL);

                        break;
                     }
                     default:
                        GUI.color = Color.cyan;
                        GUILayout.Label(updateText);
                        break;
                  }

                  GUI.color = fgColor;
               }
               EditorGUILayout.EndScrollView();

               if (updateStatus == UpdateStatus.NOT_CHECKED || updateStatus == UpdateStatus.NO_UPDATE)
               {
                  bool isChecking = !(worker == null || worker != null && !worker.IsAlive);

                  GUI.enabled = Helper.isInternetAvailable && !isChecking;

                  if (GUILayout.Button(new GUIContent(isChecking ? "Checking... Please wait." : " Check For Update", Helper.Icon_Check, "Checks for available updates of " + Constants.ASSET_NAME)))
                  {
                     worker = new System.Threading.Thread(() => UpdateCheck.UpdateCheckForEditor(out updateText, out updateStatus));
                     worker.Start();
                  }

                  GUI.enabled = true;
               }

               break;
            }
            case 0:
            {
               if (readme == null)
               {
                  string path = Application.dataPath + Config.ASSET_PATH + "README.txt";

                  try
                  {
                     readme = System.IO.File.ReadAllText(path);
                  }
                  catch (System.Exception)
                  {
                     readme = "README not found: " + path;
                  }
               }

               scrollPosAboutReadme = EditorGUILayout.BeginScrollView(scrollPosAboutReadme, false, false);
               {
                  GUILayout.Label(readme);
               }
               EditorGUILayout.EndScrollView();
               break;
            }
            default:
            {
               if (versions == null)
               {
                  string path = Application.dataPath + Config.ASSET_PATH + "Documentation/VERSIONS.txt";

                  try
                  {
                     versions = System.IO.File.ReadAllText(path);
                  }
                  catch (System.Exception)
                  {
                     versions = "VERSIONS not found: " + path;
                  }
               }

               scrollPosAboutVersions = EditorGUILayout.BeginScrollView(scrollPosAboutVersions, false, false);
               {
                  GUILayout.Label(versions);
               }

               EditorGUILayout.EndScrollView();
               break;
            }
         }

         Helper.SeparatorUI();

         GUILayout.BeginHorizontal();
         {
            if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Discord, "Communicate with us via 'Discord'.")))
               Helper.OpenURL(Constants.ASSET_SOCIAL_DISCORD);

            if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Facebook, "Follow us on 'Facebook'.")))
               Helper.OpenURL(Constants.ASSET_SOCIAL_FACEBOOK);

            if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Twitter, "Follow us on 'Twitter'.")))
               Helper.OpenURL(Constants.ASSET_SOCIAL_TWITTER);

            if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Linkedin, "Follow us on 'LinkedIn'.")))
               Helper.OpenURL(Constants.ASSET_SOCIAL_LINKEDIN);
         }
         GUILayout.EndHorizontal();

         GUILayout.Space(6);
      }

      protected static void save()
      {
         Config.Save();

         if (Config.DEBUG)
            Debug.Log("Config data saved");
      }

      protected void tpbBanner()
      {
#if !CT_TPB
         if (Constants.SHOW_TPB_BANNER)
         {
            GUILayout.BeginHorizontal();
            {
               EditorGUILayout.HelpBox("'Turbo Builder PRO' is not installed!" + System.Environment.NewLine + "For multiple&faster builds, please install or get it from the Unity AssetStore.", MessageType.Info);

               GUILayout.BeginVertical(GUILayout.Width(32));
               {
                  GUILayout.Space(4);

                  if (GUILayout.Button(new GUIContent(string.Empty, Helper.Logo_Asset_TPB, "Visit TPB in the Unity AssetStore")))
                     Helper.OpenURL(Constants.ASSET_TPB);
               }
               GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
         }
#endif
      }

      #endregion


      #region Private methods

      private static void drawColumnZebra()
      {
         if (Config.PLATFORM_WINDOWS && platformWindows)
            drawZebra(targetWindows);

         if (Config.PLATFORM_MAC && platformMac)
            drawZebra(targetMac);

         if (Config.PLATFORM_LINUX && platformLinux)
            drawZebra(targetLinux);

         if (Config.PLATFORM_ANDROID && platformAndroid)
            drawZebra(BuildTarget.Android);

         if (Config.PLATFORM_IOS && platformIOS)
            drawZebra(BuildTarget.iOS);

         if (Config.PLATFORM_WSA && platformWSA)
            drawZebra(BuildTarget.WSAPlayer);

         if (Config.PLATFORM_WEBGL && platformWebGL)
            drawZebra(BuildTarget.WebGL);

         if (Config.PLATFORM_TVOS && platformTvOS)
            drawZebra(BuildTarget.tvOS);

         if (Config.PLATFORM_PS4 && platformPS4)
            drawZebra(BuildTarget.PS4);

         if (Config.PLATFORM_XBOXONE && platformXboxOne)
            drawZebra(BuildTarget.XboxOne);

         if (Config.PLATFORM_SWITCH && platformSwitch)
            drawZebra(BuildTarget.Switch);

#if !UNITY_2018_2_OR_NEWER
         if (Config.PLATFORM_PSP2 && platformPSP2)
            drawZebra(BuildTarget.PSP2);

         if (Config.PLATFORM_WIIU && platformWiiU)
            drawZebra(BuildTarget.WiiU);

         if (Config.PLATFORM_3DS && platform3DS)
            drawZebra(BuildTarget.N3DS);
#endif
      }

      private static void drawColumnLogo()
      {
         GUILayout.BeginVertical(GUILayout.Width(logoWidth));
         {
            if (Config.PLATFORM_WINDOWS && platformWindows)
               drawLogo(Helper.Logo_Windows);

            if (Config.PLATFORM_MAC && platformMac)
               drawLogo(Helper.Logo_Mac);

            if (Config.PLATFORM_LINUX && platformLinux)
               drawLogo(Helper.Logo_Linux);

            if (Config.PLATFORM_ANDROID && platformAndroid)
               drawLogo(Helper.Logo_Android);

            if (Config.PLATFORM_IOS && platformIOS)
               drawLogo(Helper.Logo_Ios);

            if (Config.PLATFORM_WSA && platformWSA)
               drawLogo(Helper.Logo_Wsa);

            if (Config.PLATFORM_WEBGL && platformWebGL)
               drawLogo(Helper.Logo_Webgl);

            if (Config.PLATFORM_TVOS && platformTvOS)
               drawLogo(Helper.Logo_Tvos);

            if (Config.PLATFORM_PS4 && platformPS4)
               drawLogo(Helper.Logo_Ps4);

            if (Config.PLATFORM_XBOXONE && platformXboxOne)
               drawLogo(Helper.Logo_Xboxone);

            if (Config.PLATFORM_SWITCH && platformSwitch)
               drawLogo(Helper.Logo_Switch);
#if !UNITY_2018_2_OR_NEWER
            if (Config.PLATFORM_PSP2 && platformPSP2)
               drawLogo(Helper.Logo_Psp);

            if (Config.PLATFORM_WIIU && platformWiiU)
               drawLogo(Helper.Logo_Wiiu);

            if (Config.PLATFORM_3DS && platform3DS)
               drawLogo(Helper.Logo_3ds);
#endif
         }
         GUILayout.EndVertical();
      }

      private static void drawColumnPlatform()
      {
         GUILayout.BeginVertical(GUILayout.Width(platformWidth));
         {
            if (Config.PLATFORM_WINDOWS && platformWindows)
               drawPlatform("Standalone Windows");

            if (Config.PLATFORM_MAC && platformMac)
               drawPlatform("Standalone macOS");

            if (Config.PLATFORM_LINUX && platformLinux)
               drawPlatform("Standalone Linux");

            if (Config.PLATFORM_ANDROID && platformAndroid)
               drawPlatform("Android");

            if (Config.PLATFORM_IOS && platformIOS)
               drawPlatform("iOS");

            if (Config.PLATFORM_WSA && platformWSA)
               drawPlatform("UWP (WSA)");

            if (Config.PLATFORM_WEBGL && platformWebGL)
               drawPlatform("WebGL");

            if (Config.PLATFORM_TVOS && platformTvOS)
               drawPlatform("tvOS");

            if (Config.PLATFORM_PS4 && platformPS4)
               drawPlatform("PS4");

            if (Config.PLATFORM_XBOXONE && platformXboxOne)
               drawPlatform("XBoxOne");

            if (Config.PLATFORM_SWITCH && platformSwitch)
               drawPlatform("Switch");
#if !UNITY_2018_2_OR_NEWER
            if (Config.PLATFORM_PSP2 && platformPSP2)
               drawPlatform("PSP2 (Vita)");

            if (Config.PLATFORM_WIIU && platformWiiU)
               drawPlatform("WiiU");

            if (Config.PLATFORM_3DS && platform3DS)
               drawPlatform("3DS");
#endif
         }
         GUILayout.EndVertical();
      }

      private static void drawColumnArchitecture()
      {
         GUILayout.BeginVertical(GUILayout.Width(architectureWidth));
         {
            int heightSpace = 12;

            if (Config.PLATFORM_WINDOWS && platformWindows)
            {
               GUILayout.Space(heightSpace);
               Config.ARCH_WINDOWS = EditorGUILayout.Popup(string.Empty, Config.ARCH_WINDOWS, archWinOptions, GUILayout.Width(architectureWidth - 10));
               heightSpace = 18;
            }

            if (Config.PLATFORM_MAC && platformMac)
               GUILayout.Space(rowHeight);

#if !UNITY_2019_2_OR_NEWER
            if (Config.PLATFORM_LINUX && platformLinux)
            {
               GUILayout.Space(heightSpace);
               Config.ARCH_LINUX = EditorGUILayout.Popup(string.Empty, Config.ARCH_LINUX, archLinuxOptions, GUILayout.Width(architectureWidth - 10));
            }
#endif
         }
         GUILayout.EndVertical();
      }

      private static void drawColumnTexture()
      {
         GUILayout.BeginVertical(GUILayout.Width(textureWidth));
         {
            int heightSpace = 12;

            if (Config.PLATFORM_WINDOWS && platformWindows)
            {
               GUILayout.Space(heightSpace);
               heightSpace = 35;
            }

            if (Config.PLATFORM_MAC && platformMac)
            {
               GUILayout.Space(heightSpace);
               heightSpace = 35;
            }

            if (Config.PLATFORM_LINUX && platformLinux)
            {
               GUILayout.Space(heightSpace);
               heightSpace = 35;
            }

            if (Config.PLATFORM_ANDROID && platformAndroid)
            {
               GUILayout.Space(heightSpace);
               Config.TEX_ANDROID = EditorGUILayout.Popup(string.Empty, Config.TEX_ANDROID, texAndroidOptions, GUILayout.Width(textureWidth - 10));
            }
         }
         GUILayout.EndVertical();
      }

      private static void drawColumnCached()
      {
         GUILayout.BeginVertical(GUILayout.Width(cacheWidth));
         {
            if (Config.PLATFORM_WINDOWS && platformWindows)
               drawCached(targetWindows);

            if (Config.PLATFORM_MAC && platformMac)
               drawCached(targetMac);

            if (Config.PLATFORM_LINUX && platformLinux)
               drawCached(targetLinux);

            if (Config.PLATFORM_ANDROID && platformAndroid)
               drawCached(BuildTarget.Android);

            if (Config.PLATFORM_IOS && platformIOS)
               drawCached(BuildTarget.iOS);

            if (Config.PLATFORM_WSA && platformWSA)
               drawCached(BuildTarget.WSAPlayer);

            if (Config.PLATFORM_WEBGL && platformWebGL)
               drawCached(BuildTarget.WebGL);

            if (Config.PLATFORM_TVOS && platformTvOS)
               drawCached(BuildTarget.tvOS);

            if (Config.PLATFORM_PS4 && platformPS4)
               drawCached(BuildTarget.PS4);

            if (Config.PLATFORM_XBOXONE && platformXboxOne)
               drawCached(BuildTarget.XboxOne);

            if (Config.PLATFORM_SWITCH && platformSwitch)
               drawCached(BuildTarget.Switch);
#if !UNITY_2018_2_OR_NEWER
            if (Config.PLATFORM_PSP2 && platformPSP2)
               drawCached(BuildTarget.PSP2);

            if (Config.PLATFORM_WIIU && platformWiiU)
               drawCached(BuildTarget.WiiU);

            if (Config.PLATFORM_3DS && platform3DS)
               drawCached(BuildTarget.N3DS);
#endif
         }
         GUILayout.EndVertical();
      }

      private static void drawColumnAction()
      {
         GUILayout.BeginVertical();
         {
            if (Config.PLATFORM_WINDOWS && platformWindows)
               drawAction(targetWindows, Helper.Logo_Windows);

            if (Config.PLATFORM_MAC && platformMac)
               drawAction(targetMac, Helper.Logo_Mac);

            if (Config.PLATFORM_LINUX && platformLinux)
               drawAction(targetLinux, Helper.Logo_Linux);

            if (Config.PLATFORM_ANDROID && platformAndroid)
               drawAction(BuildTarget.Android, Helper.Logo_Android);

            if (Config.PLATFORM_IOS && platformIOS)
               drawAction(BuildTarget.iOS, Helper.Logo_Ios);

            if (Config.PLATFORM_WSA && platformWSA)
               drawAction(BuildTarget.WSAPlayer, Helper.Logo_Wsa);

            if (Config.PLATFORM_WEBGL && platformWebGL)
               drawAction(BuildTarget.WebGL, Helper.Logo_Webgl);

            if (Config.PLATFORM_TVOS && platformTvOS)
               drawAction(BuildTarget.tvOS, Helper.Logo_Tvos);

            if (Config.PLATFORM_PS4 && platformPS4)
               drawAction(BuildTarget.PS4, Helper.Logo_Ps4);

            if (Config.PLATFORM_XBOXONE && platformXboxOne)
               drawAction(BuildTarget.XboxOne, Helper.Logo_Xboxone);

            if (Config.PLATFORM_SWITCH && platformSwitch)
               drawAction(BuildTarget.Switch, Helper.Logo_Switch);

#if !UNITY_2018_2_OR_NEWER
            if (Config.PLATFORM_PSP2 && platformPSP2)
               drawAction(BuildTarget.PSP2, Helper.Logo_Psp);

            if (Config.PLATFORM_WIIU && platformWiiU)
               drawAction(BuildTarget.WiiU, Helper.Logo_Wiiu);

            if (Config.PLATFORM_3DS && platform3DS)
               drawAction(BuildTarget.N3DS, Helper.Logo_3ds);
#endif
         }
         GUILayout.EndVertical();
      }

      private static void drawVerticalSeparator(bool title = false)
      {
         GUILayout.BeginVertical(GUILayout.Width(2));
         {
            GUILayout.Box(string.Empty, title
               ? new[]
               {
                  GUILayout.Height(24), GUILayout.Width(1)
               }
               : new[] {GUILayout.Height(platformY + rowHeight - 4), GUILayout.Width(1)});
         }
         GUILayout.EndVertical();
      }

      private static void drawZebra(BuildTarget target)
      {
         platformY += rowHeight;
         rowHeight = 36;

         if (EditorUserBuildSettings.activeBuildTarget == target)
         {
            Color currentPlatform = new Color(0f, 0.33f, 0.71f); //CT-blue

            if (target == BuildTarget.Android)
            {
               if (EditorUserBuildSettings.androidBuildSubtarget == texAndroid)
               {
                  EditorGUI.DrawRect(new Rect(platformX, platformY, rowWidth, rowHeight), currentPlatform);
               }
               else
               {
                  if (rowCounter % 2 == 0)
                  {
                     EditorGUI.DrawRect(new Rect(platformX, platformY, rowWidth, rowHeight), Color.gray);
                  }
               }
            }
            else
            {
               EditorGUI.DrawRect(new Rect(platformX, platformY, rowWidth, rowHeight), currentPlatform);
            }
         }
         else
         {
            if (rowCounter % 2 == 0)
            {
               EditorGUI.DrawRect(new Rect(platformX, platformY, rowWidth, rowHeight), Color.gray);
            }
         }

         rowCounter++;
      }

      private static void drawLogo(Texture logo)
      {
         platformY += rowHeight;
         rowHeight = 36;

         GUILayout.Label(string.Empty);

         GUI.DrawTexture(new Rect(platformX + 4, platformY + 4, 28, 28), logo);
      }

      private static void drawPlatform(string label)
      {
         GUILayout.Space(platformTextSpace);
         GUILayout.Label(label /*, EditorStyles.boldLabel */);

         platformTextSpace = 18;
      }

      private static void drawCached(BuildTarget target)
      {
         GUILayout.Space(cacheTextSpace);

         GUILayout.Label(Helper.isCached(target, texAndroid)
            ? new GUIContent(string.Empty, Helper.Icon_Cachefull, "Cached: " + target)
            : new GUIContent(string.Empty, Helper.Icon_Cacheempty, "Not cached: " + target));

         cacheTextSpace = 11;
      }

      private static void drawAction(BuildTarget target, Texture logo)
      {
         GUILayout.Space(actionTextSpace);

         GUILayout.BeginHorizontal();
         {
            GUI.enabled = !Helper.isDeleting;

            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
               if (target == BuildTarget.Android && !Helper.isDeleting)
               {
                  GUI.enabled = EditorUserBuildSettings.androidBuildSubtarget != texAndroid;
               }
               else
               {
                  GUI.enabled = false;
               }
            }

            string targetName = target == BuildTarget.Android ? target + " (" + texAndroid + ")" : target.ToString();

            if (GUILayout.Button(new GUIContent(string.Empty, logo, " Switch to " + targetName)))
            {
               if (!Config.CONFIRM_SWITCH || EditorUtility.DisplayDialog("Switch to " + targetName + "?",
                  (Config.USE_LEGACY ? Constants.ASSET_NAME + " will now close Unity and sync the following folders: " : "Sync the following folders: ") + System.Environment.NewLine +
                  (Config.COPY_ASSETS ? "• Assets" + System.Environment.NewLine : string.Empty) +
                  (Config.COPY_LIBRARY ? "• Library" + System.Environment.NewLine : string.Empty) +
                  (Config.COPY_SETTINGS ? "• ProjectSettings" + System.Environment.NewLine : string.Empty) +
                  System.Environment.NewLine +
                  "Cache directory: " + Config.PATH_CACHE +
                  System.Environment.NewLine +
                  System.Environment.NewLine +
                  "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to start the switch?", "Yes", "No"))
               {
                  if (Config.DEBUG)
                     Debug.Log("Switch initiated: " + targetName);

                  save();

                  Switcher.Switch(target, target == BuildTarget.Android ? texAndroid : MobileTextureSubtarget.Generic);
               }
            }

            if (Config.SHOW_DELETE && Helper.isCached(target, texAndroid))
            {
               if (GUILayout.Button(new GUIContent(string.Empty, Helper.Icon_Delete_Big, "Delete cache from " + target)))
               {
                  if (EditorUtility.DisplayDialog("Delete the cache for " + target + "?", "If you delete the cache, Unity must re-import all assets for this platform after a switch." + System.Environment.NewLine + "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to delete the cache?", "Yes", "No"))
                  {
                     if (Config.DEBUG)
                        Debug.Log("Cache deleted: " + target);

                     Helper.DeleteCacheFromTarget(target, texAndroid);
                  }
               }
            }

            GUI.enabled = true;
         }
         GUILayout.EndHorizontal();

         actionTextSpace = 8;
      }

      private static void drawHeader()
      {
         GUILayout.Space(6);
         GUILayout.BeginHorizontal();
         {
            if (Config.SHOW_COLUMN_PLATFORM)
            {
               GUILayout.BeginVertical(GUILayout.Width(platformWidth + (Config.SHOW_COLUMN_PLATFORM_LOGO ? logoWidth + 4 : 0)));
               {
                  GUILayout.Label(new GUIContent("Platform", "Platform name"), EditorStyles.boldLabel);
               }
               GUILayout.EndVertical();

               drawVerticalSeparator(true);
            }

            if (Config.SHOW_COLUMN_ARCHITECTURE && Helper.hasActiveArchitecturePlatforms)
            {
               GUILayout.BeginVertical(GUILayout.Width(architectureWidth));
               {
                  GUILayout.Label(new GUIContent("Arch", "Architecture of the target platform."), EditorStyles.boldLabel);
               }
               GUILayout.EndVertical();

               drawVerticalSeparator(true);
            }

            if (Config.SHOW_COLUMN_TEXTURE && Helper.hasActiveTexturePlatforms)
            {
               GUILayout.BeginVertical(GUILayout.Width(textureWidth));
               {
                  GUILayout.Label(new GUIContent("Texture", "Texture format"), EditorStyles.boldLabel);
               }
               GUILayout.EndVertical();

               drawVerticalSeparator(true);
            }

            if (Config.SHOW_COLUMN_CACHE)
            {
               GUILayout.BeginVertical(GUILayout.Width(cacheWidth));
               {
                  GUILayout.Label(new GUIContent("Cache", "Cache-status of the platform."), EditorStyles.boldLabel);
               }
               GUILayout.EndVertical();

               drawVerticalSeparator(true);
            }

            GUILayout.BeginVertical(GUILayout.Width(actionWidth));
            {
               GUILayout.Label(new GUIContent("Action", "Action for the platform."), EditorStyles.boldLabel);
            }
            GUILayout.EndVertical();
         }
         GUILayout.EndHorizontal();

         Helper.SeparatorUI(0);
      }

      private static void drawContent()
      {
         GUILayout.BeginHorizontal();
         {
            drawColumnZebra();

            if (Config.SHOW_COLUMN_PLATFORM)
            {
               if (Config.SHOW_COLUMN_PLATFORM_LOGO)
               {
                  platformY = 0;
                  rowHeight = 0;
                  drawColumnLogo();
               }

               drawColumnPlatform();

               drawVerticalSeparator();
            }

            if (Config.SHOW_COLUMN_ARCHITECTURE && Helper.hasActiveArchitecturePlatforms)
            {
               drawColumnArchitecture();

               drawVerticalSeparator();
            }

            if (Config.SHOW_COLUMN_TEXTURE && Helper.hasActiveTexturePlatforms)
            {
               drawColumnTexture();

               drawVerticalSeparator();
            }

            if (Config.SHOW_COLUMN_CACHE)
            {
               drawColumnCached();

               drawVerticalSeparator();
            }

            drawColumnAction();
         }
         GUILayout.EndHorizontal();
      }

      #endregion
   }
}
#endif
// © 2016-2020 crosstales LLC (https://www.crosstales.com)
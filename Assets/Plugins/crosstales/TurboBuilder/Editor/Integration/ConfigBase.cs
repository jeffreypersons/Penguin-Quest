#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Crosstales.TPB.Util;
using Crosstales.TPB.Task;

namespace Crosstales.TPB.EditorIntegration
{
   /// <summary>Base class for editor windows.</summary>
   public abstract class ConfigBase : EditorWindow
   {
      #region Variables

      private static string updateText = UpdateCheck.TEXT_NOT_CHECKED;
      private static UpdateStatus updateStatus = UpdateStatus.NOT_CHECKED;

      private System.Threading.Thread worker;

      private Vector2 scrollPosBuild;
      private Vector2 scrollPosPlatforms;
      private Vector2 scrollPosConfig;
      private Vector2 scrollPosHelp;
      private Vector2 scrollPosAboutUpdate;
      private Vector2 scrollPosAboutReadme;
      private Vector2 scrollPosAboutVersions;

      private static string readme;
      private static string versions;

      private int aboutTab = 0;

      private const int space = 4;

      private static readonly string[] vcsOptions = {"None", "git", "SVN", "Mercurial", "Collab", "PlasticSCM"};

      private const int rowWidth = 10000;
      private static int rowHeight = 0; //later = 36
      private static int rowCounter = 0;

      private const int logoWidth = 36;
      private const int platformWidth = 128;
      private const int architectureWidth = 64;
      private const int actionWidth = 48;

      private static int platformX = 0;
      private static int platformY = 0;
      private static int platformTextSpace = 12; //later = 18

      private static int actionTextSpace = 6; //later = 8

      private static readonly string[] archWinOptions = {"32bit", "64bit"};
      private static readonly string[] archLinuxOptions = {"32bit", "64bit", "Universal"};

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

      protected void showBuild()
      {
         //tpsBanner();

         GUI.skin.label.wordWrap = true;

         if (Helper.isEditorMode)
         {
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
               if (Helper.Targets.Count > 0)
               {
                  platformX = 0;
                  platformY = 0;
                  platformTextSpace = 12; //later = 18

                  rowHeight = 0; //later = 36
                  rowCounter = 0;

                  actionTextSpace = 6; //later = 8

                  // header
                  drawHeader();

                  scrollPosBuild = EditorGUILayout.BeginScrollView(scrollPosBuild, false, false);
                  {
                     //content
                     drawContent();
                  }
                  EditorGUILayout.EndScrollView();

                  if (!Helper.hasActiveScenes)
                     EditorGUILayout.HelpBox("No active scenes found - build not possible!" + System.Environment.NewLine + "Please add and enable at least one scene.", MessageType.Error);

                  GUI.enabled = Helper.hasActiveScenes;

                  if (GUILayout.Button(new GUIContent(" Build all", Helper.Icon_Play, "Create builds for all platforms.")))
                  {
#if CT_TPS
                        if (!Config.CONFIRM_BUILD || TPS.Util.Config.USE_LEGACY ? EditorUtility.DisplayDialog("Build all platforms?", "Create builds for:" + System.Environment.NewLine + Helper.Targets.CTDump("• ") + System.Environment.NewLine + System.Environment.NewLine + "Unity will now close, build all platforms and then restart." + System.Environment.NewLine + "This operation could take some time." + System.Environment.NewLine +  System.Environment.NewLine + "Would you like to start the build process?", "Yes", "No") : EditorUtility.DisplayDialog("Build all platforms?", "Create builds for:" + System.Environment.NewLine + Helper.Targets.CTDump("• ") + System.Environment.NewLine + System.Environment.NewLine + "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to start the build process?", "Yes", "No"))
#else
                     if (!Config.CONFIRM_BUILD || EditorUtility.DisplayDialog("Build all platforms?", "Create builds for:" + System.Environment.NewLine + Helper.Targets.CTDump("• ") + System.Environment.NewLine + System.Environment.NewLine + "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to start the build process?", "Yes", "No"))
#endif
                     {
                        save();

                        Builder.BuildAll();
                     }
                  }

                  GUI.enabled = true;

                  GUILayout.Space(6);
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
         tpsBanner();

         scrollPosPlatforms = EditorGUILayout.BeginScrollView(scrollPosPlatforms, false, false);
         {
            GUILayout.Label("General Settings", EditorStyles.boldLabel);
            Config.CUSTOM_PATH_BUILD = EditorGUILayout.BeginToggleGroup(new GUIContent("Custom Build Path", "Enable or disable a custom build path (default: " + Constants.DEFAULT_CUSTOM_PATH_BUILD + ")."), Config.CUSTOM_PATH_BUILD);
            {
               EditorGUI.indentLevel++;

               EditorGUILayout.BeginHorizontal();
               {
                  EditorGUILayout.SelectableLabel(Config.PATH_BUILD);

                  if (GUILayout.Button(new GUIContent(" Select", Helper.Icon_Folder, "Select path for the builds")))
                  {
                     string path = EditorUtility.OpenFolderPanel("Select path for the builds", Config.PATH_BUILD.Substring(0, Config.PATH_BUILD.Length - (Constants.BUILD_DIRNAME.Length + 1)), "");

                     if (!string.IsNullOrEmpty(path))
                     {
                        Config.PATH_BUILD = path + "/" + Constants.BUILD_DIRNAME;
                     }
                  }
               }
               EditorGUILayout.EndHorizontal();

               EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndToggleGroup();

            //GUILayout.Space(space);
            GUI.enabled = !Config.CUSTOM_PATH_BUILD;

            Config.VCS = EditorGUILayout.Popup("Version Control", Config.VCS, vcsOptions);

            GUILayout.Space(space);
            GUI.enabled = true;

            Config.ADD_NAME_TO_PATH = EditorGUILayout.Toggle(new GUIContent("Add Name To Path", "Enable or disable adding the product name to the build path (default: " + Constants.DEFAULT_ADD_NAME_TO_PATH + ")"), Config.ADD_NAME_TO_PATH);
            Config.ADD_DATE_TO_PATH = EditorGUILayout.Toggle(new GUIContent("Add Date To Path", "Enable or disable adding the current date and time to the build path (default: " + Constants.DEFAULT_ADD_DATE_TO_PATH + ")"), Config.ADD_DATE_TO_PATH);

            /*
            Config.BATCHMODE = EditorGUILayout.BeginToggleGroup(new GUIContent("Batch mode", "Enable or disable batch mode for CLI operations (default: " + Constants.DEFAULT_BATCHMODE + ")"), Config.BATCHMODE);
            {
                EditorGUI.indentLevel++;

                Config.QUIT = EditorGUILayout.Toggle(new GUIContent("Quit", "Enable or disable quit Unity Editor for CLI operations (default: " + Constants.DEFAULT_QUIT + ")."), Config.QUIT);

                Config.NO_GRAPHICS = EditorGUILayout.Toggle(new GUIContent("No graphics", "Enable or disable graphics device in Unity Editor for CLI operations (default: " + Constants.DEFAULT_NO_GRAPHICS + ")."), Config.NO_GRAPHICS);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndToggleGroup();
            */

#if CT_TPS
                if (TPS.Util.Config.USE_LEGACY)
                    Config.DELETE_LOCKFILE = EditorGUILayout.Toggle(new GUIContent("Delete UnityLockfile", "Enable or disable deleting the 'UnityLockfile' (default: " + Constants.DEFAULT_DELETE_LOCKFILE + ")."), Config.DELETE_LOCKFILE);
#endif
            Config.DEBUG = EditorGUILayout.Toggle(new GUIContent("Debug", "Enable or disable debug logs (default: " + Constants.DEFAULT_DEBUG + ")."), Config.DEBUG);

            Config.UPDATE_CHECK = EditorGUILayout.Toggle(new GUIContent("Update Check", "Enable or disable the update-checks for the asset (default: " + Constants.DEFAULT_UPDATE_CHECK + ")"), Config.UPDATE_CHECK);

            Config.COMPILE_DEFINES = EditorGUILayout.Toggle(new GUIContent("Compile Defines", "Enable or disable adding compile define 'CT_TPB' for the asset (default: " + Constants.DEFAULT_COMPILE_DEFINES + ")"), Config.COMPILE_DEFINES);

            Helper.SeparatorUI();

            GUILayout.Label("Build Settings", EditorStyles.boldLabel);
            Config.EXECUTE_METHOD_PRE_BUILDING = EditorGUILayout.TextField(new GUIContent("Method Before Building", "Execute static method <ClassName.MethodName> in Unity before building (default: empty)."), Config.EXECUTE_METHOD_PRE_BUILDING);
            Config.EXECUTE_METHOD_POST_BUILDING = EditorGUILayout.TextField(new GUIContent("Method After Building", "Execute static method <ClassName.MethodName> in Unity after building (default: empty)."), Config.EXECUTE_METHOD_POST_BUILDING);

            Config.EXECUTE_METHOD_PRE_BUILD = EditorGUILayout.TextField(new GUIContent("Method Before Build", "Execute static method <ClassName.MethodName> in Unity before a build (default: empty)."), Config.EXECUTE_METHOD_PRE_BUILD);
            Config.EXECUTE_METHOD_POST_BUILD = EditorGUILayout.TextField(new GUIContent("Method After Build", "Execute static method <ClassName.MethodName> in Unity after a build (default: empty)."), Config.EXECUTE_METHOD_POST_BUILD);

            Config.BO_SHOW_BUILT_PLAYER = EditorGUILayout.Toggle(new GUIContent("Show Built Player", "Enable or disable 'BuildOptions.ShowBuiltPlayer' (default: " + Constants.DEFAULT_BO_SHOW_BUILT_PLAYER + ")."), Config.BO_SHOW_BUILT_PLAYER);
            Config.BO_DEVELOPMENT = EditorGUILayout.Toggle(new GUIContent("Development Build", "Enable or disable 'BuildOptions.Development' (default: " + Constants.DEFAULT_BO_DEVELOPMENT + ")."), Config.BO_DEVELOPMENT);
            Config.BO_PROFILER = EditorGUILayout.Toggle(new GUIContent("Autoconnect Profiler", "Enable or disable 'BuildOptions.ConnectWithProfiler' (default: " + Constants.DEFAULT_BO_PROFILER + ")."), Config.BO_PROFILER);
            Config.BO_SCRIPTDEBUG = EditorGUILayout.Toggle(new GUIContent("Script Debugging", "Enable or disable 'BuildOptions.AllowDebugging' (default: " + Constants.DEFAULT_BO_SCRIPTDEBUG + ")."), Config.BO_SCRIPTDEBUG);

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

            //BuildPipeline.BuildPlayer(ScenePaths, playerPath, target, BuildOptions.ShowBuiltPlayer | BuildOptions.Development | BuildOptions.ConnectWithProfiler); AllowDebugging

            GUILayout.Label("UI Settings", EditorStyles.boldLabel);
            Config.CONFIRM_BUILD = EditorGUILayout.Toggle(new GUIContent("Confirm Build", "Enable or disable the build confirmation dialog (default: " + Constants.DEFAULT_CONFIRM_BUILD + ")."), Config.CONFIRM_BUILD);
            Config.SHOW_COLUMN_PLATFORM = EditorGUILayout.Toggle(new GUIContent("Column: Platform", "Enable or disable the column 'Platform' in the 'Build'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_PLATFORM + ")."), Config.SHOW_COLUMN_PLATFORM);
            Config.SHOW_COLUMN_ARCHITECTURE = EditorGUILayout.Toggle(new GUIContent("Column: Arch", "Enable or disable the column 'Arch' in the 'Build'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_ARCHITECTURE + ")."), Config.SHOW_COLUMN_ARCHITECTURE);
            //Config.SHOW_COLUMN_TEXTURE = EditorGUILayout.Toggle(new GUIContent("Column: Texture", "Enable or disable the column 'Texture' in the 'Build'-aboutTab (default: " + Constants.DEFAULT_SHOW_COLUMN_TEXTURE + ")."), Config.SHOW_COLUMN_TEXTURE);
         }
         EditorGUILayout.EndScrollView();
         Helper.SeparatorUI();

         GUILayout.Label("Build Usage", EditorStyles.boldLabel);

         GUI.skin.label.wordWrap = true;

         GUILayout.Label(Helper.BuildInfo);

         GUI.skin.label.wordWrap = false;

         GUI.enabled = Helper.hasBuild && !Helper.isDeleting;

         if (GUILayout.Button(new GUIContent(" Show Builds", Helper.Icon_Show, "Show all builds.")))
         {
            Helper.ShowFileLocation(Config.PATH_BUILD);
         }

         if (GUILayout.Button(new GUIContent(" Delete Builds", Helper.Icon_Delete, "Delete all builds.")))
         {
            if (EditorUtility.DisplayDialog("Delete all builds?", "Would you like to delete all builds?", "Yes", "No"))
            {
               Helper.DeleteBuilds();

               if (Config.DEBUG)
                  Debug.Log("All builds deleted");
            }
         }

         GUI.enabled = true;
      }

      protected void showHelp()
      {
         tpsBanner();

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
         tpsBanner();

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

         GUILayout.Label("© 2018-2020 by " + Constants.ASSET_AUTHOR);

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

         if (aboutTab == 2)
         {
            scrollPosAboutUpdate = EditorGUILayout.BeginScrollView(scrollPosAboutUpdate, false, false);
            {
               Color fgColor = GUI.color;

               GUI.color = Color.yellow;

               if (updateStatus == UpdateStatus.NO_UPDATE)
               {
                  GUI.color = Color.green;
                  GUILayout.Label(updateText);
               }
               else if (updateStatus == UpdateStatus.UPDATE)
               {
                  GUILayout.Label(updateText);

                  if (GUILayout.Button(new GUIContent(" Download", "Visit the 'Unity AssetStore' to download the latest version.")))
                     UnityEditorInternal.AssetStore.Open("content/" + Constants.ASSET_ID);
               }
               else if (updateStatus == UpdateStatus.UPDATE_VERSION)
               {
                  GUILayout.Label(updateText);

                  if (GUILayout.Button(new GUIContent(" Upgrade", "Upgrade to the newer version in the 'Unity AssetStore'")))
                     Helper.OpenURL(Constants.ASSET_CT_URL);
               }
               else if (updateStatus == UpdateStatus.DEPRECATED)
               {
                  GUILayout.Label(updateText);

                  if (GUILayout.Button(new GUIContent(" More Information", "Visit the 'crosstales'-site for more information.")))
                     Helper.OpenURL(Constants.ASSET_AUTHOR_URL);
               }
               else
               {
                  GUI.color = Color.cyan;
                  GUILayout.Label(updateText);
               }

               GUI.color = fgColor;
            }
            EditorGUILayout.EndScrollView();

            if (updateStatus == UpdateStatus.NOT_CHECKED || updateStatus == UpdateStatus.NO_UPDATE)
            {
               bool isChecking = !(worker == null || (worker != null && !worker.IsAlive));

               GUI.enabled = Helper.isInternetAvailable && !isChecking;

               if (GUILayout.Button(new GUIContent(isChecking ? "Checking... Please wait." : " Check For Update", Helper.Icon_Check, "Checks for available updates of " + Constants.ASSET_NAME)))
               {
                  worker = new System.Threading.Thread(() => UpdateCheck.UpdateCheckForEditor(out updateText, out updateStatus));
                  worker.Start();
               }

               GUI.enabled = true;
            }
         }
         else if (aboutTab == 0)
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
         }
         else
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

      protected void tpsBanner()
      {
#if !CT_TPS
         if (Constants.SHOW_TPS_BANNER)
         {
            GUILayout.BeginHorizontal();
            {
               EditorGUILayout.HelpBox("'Turbo Switch PRO' is not installed!" + System.Environment.NewLine + "For faster builds, please install or get it from the Unity AssetStore.", MessageType.Info);

               GUILayout.BeginVertical(GUILayout.Width(32));
               {
                  GUILayout.Space(4);

                  if (GUILayout.Button(new GUIContent(string.Empty, Helper.Logo_Asset_TPS, "Visit TPS in the Unity AssetStore")))
                     Helper.OpenURL(Constants.ASSET_TPS);
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
            drawZebra(Helper.TargetWindows);

         if (Config.PLATFORM_MAC && platformMac)
            drawZebra(Helper.TargetMac);

         if (Config.PLATFORM_LINUX && platformLinux)
            drawZebra(Helper.TargetLinux);

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
               Config.ARCH_WINDOWS = EditorGUILayout.Popup(string.Empty, Config.ARCH_WINDOWS, archWinOptions, new GUILayoutOption[] {GUILayout.Width(architectureWidth - 10)});
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

      private static void drawColumnAction()
      {
         GUILayout.BeginVertical();
         {
            if (Config.PLATFORM_WINDOWS && platformWindows)
               drawAction(Helper.TargetWindows, Helper.Logo_Windows);

            if (Config.PLATFORM_MAC && platformMac)
               drawAction(Helper.TargetMac, Helper.Logo_Mac);

            if (Config.PLATFORM_LINUX && platformLinux)
               drawAction(Helper.TargetLinux, Helper.Logo_Linux);

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
            if (title)
            {
               GUILayout.Box(string.Empty, new GUILayoutOption[]
               {
                  /*GUILayout.ExpandHeight(true)*/ GUILayout.Height(24), GUILayout.Width(1)
               });
            }
            else
            {
               GUILayout.Box(string.Empty, new GUILayoutOption[] {GUILayout.Height(platformY + rowHeight - 4), GUILayout.Width(1)});
            }
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

            EditorGUI.DrawRect(new Rect(platformX, platformY, rowWidth, rowHeight), currentPlatform);
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

      private static void drawLogo(Texture2D logo)
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

      private static void drawAction(BuildTarget target, Texture2D logo)
      {
         GUILayout.Space(actionTextSpace);

         //string targetName = target == BuildTarget.Android ? target + " (" + texAndroid + ")" : target.ToString();
         string targetName = target.ToString();

         GUI.enabled = Helper.hasActiveScenes;

         if (GUILayout.Button(new GUIContent(string.Empty, logo, " Build " + targetName)))
         {
            save();

            //if (!Config.CONFIRM_BUILD || EditorUtility.DisplayDialog("Build " + target + "?", "Would you like to build the platform?", "Yes, build " + target, "Cancel"))
            if (!Config.CONFIRM_BUILD || EditorUtility.DisplayDialog("Build " + target + "?", "Would you like to build the platform?", "Yes", "No"))
            {
               Builder.Build(target);
            }
         }

         GUI.enabled = true;

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

            drawColumnAction();
         }
         GUILayout.EndHorizontal();
      }

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
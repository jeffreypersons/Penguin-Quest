#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Crosstales.TPB.Util;

//using Ionic.Zip; // this uses the Unity port of DotNetZip https://github.com/r2d2rigo/dotnetzip-for-unity

namespace Crosstales.TPB
{
   /// <summary>Platform builder.</summary>
   public class Builder
   {
      /// <summary>The current build target.</summary>
      public static BuildTarget CurrentBuildTarget = BuildTarget.NoTarget;

      private static string filename = Application.productName; //CHANGE: set your desired filename
      private static bool deleteBuild = true; //CHANGE: set to true you want to delete the previous build


      #region Public methods

      /// <summary>Builds the given target.</summary>
      /// <param name="target">Build target</param>
      /// <param name="path">Build path (optional)</param>
      /// <param name="name">Name of the build artifact (optional)</param>
      /// <returns>True if the build was successful.</returns>
      public static bool Build(BuildTarget target, string path = null, string name = null)
      {
         Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_BATCHMODE, false);
         Common.Util.CTPlayerPrefs.Save();

         return build(false, path, name, Helper.getBuildNameFromBuildTarget(target));
      }

      /// <summary>Builds all selected targets.</summary>
      /// <param name="path">Build path (optional)</param>
      /// <param name="name">Name of the build artifact (optional)</param>
      /// <returns>True if the builds were successful.</returns>
      public static bool BuildAll(string path = null, string name = null)
      {
         Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_BATCHMODE, false);
         Common.Util.CTPlayerPrefs.Save();

         System.Collections.Generic.List<string> _targets = new System.Collections.Generic.List<string>();

         foreach (BuildTarget t in Helper.Targets)
         {
            _targets.Add(Helper.getBuildNameFromBuildTarget(t));
         }

         return build(false, path, name, _targets.ToArray());
      }

      /// <summary>Builds all selected targets via CLI.</summary>
      public static void BuildAllCLI()
      {
         Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_BATCHMODE, true);
         Common.Util.CTPlayerPrefs.Save();

         System.Collections.Generic.List<string> _targets = new System.Collections.Generic.List<string>();

         foreach (BuildTarget t in Helper.Targets)
         {
            _targets.Add(Helper.getBuildNameFromBuildTarget(t));
         }

         //TODO add parameters

         string _path = Helper.getCLIArgument("-tpbPath");
         string _name = Helper.getCLIArgument("-tpbName");

         build(true, _path, _name, _targets.ToArray());
      }

      /// <summary>Builds the targets via CLI.</summary>
      public static void BuildCLI()
      {
         Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_BATCHMODE, true);
         Common.Util.CTPlayerPrefs.Save();

         //TODO add parameters

         string[] _targets = Helper.getCLIArgument("-tpbTargets").Split(new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);
         string _path = Helper.getCLIArgument("-tpbPath");
         string _name = Helper.getCLIArgument("-tpbName");

         build(true, _path, _name, _targets);
      }

#if CT_TPS
     /// <summary>Builds the current target via TPS.</summary>
     public static void BuildTPS()
     {
         if (Common.Util.CTPlayerPrefs.HasKey(Constants.KEY_BATCHMODE))
         {
             bool forceBatchmode = Common.Util.CTPlayerPrefs.GetBool(Constants.KEY_BATCHMODE);

             if (Common.Util.CTPlayerPrefs.HasKey(Constants.KEY_TARGETS))
             {
                 string[] targets = Common.Util.CTPlayerPrefs.GetString(Constants.KEY_TARGETS).Split(';');

                 if (targets.Length > 0)
                 {
                     if (targets.Length == 1)
                     {
                         Common.Util.CTPlayerPrefs.SetString(Constants.KEY_TARGETS, string.Empty);
                     }
                     else
                     {
                         string result = targets[1];

                         for (int ii = 2; ii < targets.Length; ii++)
                         {
                             result += ";" + targets[ii];
                         }

                         Common.Util.CTPlayerPrefs.SetString(Constants.KEY_TARGETS, result);
                     }

                     Common.Util.CTPlayerPrefs.Save();

                     //CTLogger.Log(EditorUserBuildSettings.activeBuildTarget.ToString());

                     //bool forceBatchmode = "true".CTEquals(Helper.getCLIArgument("-tpsBatchmode")) ? true : false;
                     buildPlayer(EditorUserBuildSettings.activeBuildTarget, filename, Config.PATH_BUILD); //TODO set filename&path?

                     //CTLogger.Log("targets[0]: '" + targets[0] + "' - " + targets.Length + " - " + forceBatchmode);

                     if (!string.IsNullOrEmpty(targets[0]))
                     {
                         Helper.ProcessBuildPipeline(targets[0], targets.Length > 1 || forceBatchmode);

                         //CTLogger.Log("Process");
                     }
                     else
                     {
                         //CTLogger.Log("Finish");

                         if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_POST_BUILDING))
                             Helper.InvokeMethod(Config.EXECUTE_METHOD_POST_BUILDING.Substring(0, Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".")), Config.EXECUTE_METHOD_POST_BUILDING.Substring(Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".") + 1));

                         if (forceBatchmode)
                             EditorApplication.Exit(0);
                     }
                 }
             }
         }
     }
#endif

      /// <summary>Test building with an execute method.</summary>
      public static void SayHello()
      {
         Debug.LogError("Hello everybody, I was called by " + Constants.ASSET_NAME);

         if (Config.DEBUG)
            Debug.Log("CurrentBuildTarget: " + CurrentBuildTarget);
      }

      /// <summary>Test method (before building).</summary>
      public static void MethodBeforeBuilding()
      {
         Debug.LogWarning("'MethodBeforeBuilding' was called!");
      }

      /// <summary>Test method (after building).</summary>
      public static void MethodAfterBuilding()
      {
         Debug.LogWarning("'MethodAfterBuilding' was called");
      }

      /// <summary>Test method (before a build).</summary>
      public static void MethodBeforeBuild()
      {
         Debug.LogWarning("'MethodBeforeBuild' was called!");
      }

      /// <summary>Test method (after a build).</summary>
      public static void MethodAfterBuild()
      {
         Debug.LogWarning("'MethodAfterBuild' was called: " + CurrentBuildTarget);
      }

      #endregion


      #region Private methods

      private static bool build(bool quit, string path = null, string name = null, params string[] targets)
      {
         UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

         string _path = string.IsNullOrEmpty(path) ? Config.PATH_BUILD : path;
         string _filename = string.IsNullOrEmpty(name) ? filename : name;

         Helper.SetupVCS();

         bool success = true;

         if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_PRE_BUILDING))
            Helper.InvokeMethod(Config.EXECUTE_METHOD_PRE_BUILDING.Substring(0, Config.EXECUTE_METHOD_PRE_BUILDING.LastIndexOf(".")), Config.EXECUTE_METHOD_PRE_BUILDING.Substring(Config.EXECUTE_METHOD_PRE_BUILDING.LastIndexOf(".") + 1));

         System.Collections.Generic.List<string> btList = new System.Collections.Generic.List<string>();

         //optimization: always use active platform first
         BuildTarget bt;
         foreach (string target in targets)
         {
            bt = Helper.getBuildTargetForBuildName(target);

            if (EditorUserBuildSettings.activeBuildTarget == bt)
            {
               buildPlayer(bt, _filename, _path); //TODO quit??
            }
            else
            {
               btList.Add(target);
            }
         }
#if CT_TPS
         if (TPS.Util.Config.USE_LEGACY)
         {
             if (btList.Count > 1)
             {
                 string result = btList[1];

                 for (int ii = 2; ii < btList.Count; ii++)
                 {
                     result += ";" + btList[ii];
                 }

                 Common.Util.CTPlayerPrefs.SetString(Constants.KEY_TARGETS, result);
             }
             else
             {
                 Common.Util.CTPlayerPrefs.SetString(Constants.KEY_TARGETS, string.Empty);

                 if (btList.Count == 0 && !string.IsNullOrEmpty(Config.EXECUTE_METHOD_POST_BUILDING))
                     Helper.InvokeMethod(
                         Config.EXECUTE_METHOD_POST_BUILDING.Substring(0,
                             Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".")),
                         Config.EXECUTE_METHOD_POST_BUILDING.Substring(
                             Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".") + 1));
             }

             Common.Util.CTPlayerPrefs.Save();

             //CTLogger.Log("btList[0]: '" + btList[0] + "' - " + btList.Count + " - " + quit);

             if (btList.Count > 0)
                 Helper.ProcessBuildPipeline(btList[0], btList.Count > 1 || quit);
         }
         else
         {
             foreach (string target in btList)
             {
                 TPS.Switcher.Switch(Helper.getBuildTargetForBuildName(target));
                 if (!buildPlayer(Helper.getBuildTargetForBuildName(target), _filename, _path)) //TODO quit??
                    success = false;
             }

             if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_POST_BUILDING))
                 Helper.InvokeMethod(Config.EXECUTE_METHOD_POST_BUILDING.Substring(0, Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".")), Config.EXECUTE_METHOD_POST_BUILDING.Substring(Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".") + 1));

             if (quit)
                 EditorApplication.Exit(0);
         }
#else
         foreach (string target in btList)
         {
            if (!buildPlayer(Helper.getBuildTargetForBuildName(target), _filename, _path)) //TODO quit??
               success = false;
         }

         if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_POST_BUILDING))
            Helper.InvokeMethod(Config.EXECUTE_METHOD_POST_BUILDING.Substring(0, Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".")), Config.EXECUTE_METHOD_POST_BUILDING.Substring(Config.EXECUTE_METHOD_POST_BUILDING.LastIndexOf(".") + 1));

         if (quit)
            EditorApplication.Exit(0);
#endif

         return success;
      }

      /// <summary>Builds the platform target.</summary>
      /// <param name="target">Target platform for the build</param>
      /// <param name="name">File name of the build.</param>
      /// <param name="path">Path for the build.</param>
      // <param name="quit">Quit Unity after the build is completed (default: false, optional)</param>
      private static bool buildPlayer(BuildTarget target, string name, string path) //, bool quit = false)
      {
         bool success = false;

         if (Helper.hasActiveScenes)
         {
            CurrentBuildTarget = target;

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_PRE_BUILD))
               Helper.InvokeMethod(Config.EXECUTE_METHOD_PRE_BUILD.Substring(0, Config.EXECUTE_METHOD_PRE_BUILD.LastIndexOf(".")), Config.EXECUTE_METHOD_PRE_BUILD.Substring(Config.EXECUTE_METHOD_PRE_BUILD.LastIndexOf(".") + 1));

            string fileExtension = string.Empty;
            string modifier;

            // configure path variables based on the platform we're targeting
            switch (target)
            {
               case BuildTarget.StandaloneWindows:
                  modifier = "win32";
                  fileExtension = ".exe";
                  break;
               case BuildTarget.StandaloneWindows64:
                  modifier = "win64";
                  fileExtension = ".exe";
                  break;
               case BuildTarget.StandaloneOSX:
                  modifier = "mac";
                  fileExtension = ".app";
                  break;
#if !UNITY_2019_2_OR_NEWER
               case BuildTarget.StandaloneLinux:
               case BuildTarget.StandaloneLinuxUniversal:
#endif
               case BuildTarget.StandaloneLinux64:
                  modifier = "linux";
                  switch (target)
                  {
#if !UNITY_2019_2_OR_NEWER
                     case BuildTarget.StandaloneLinux:
                        fileExtension = ".x86";
                        break;
                     case BuildTarget.StandaloneLinuxUniversal:
                        fileExtension = ".x86_64";
                        break;
#endif
                     case BuildTarget.StandaloneLinux64:
                        fileExtension = ".x64";
                        break;
                  }

                  break;
               case BuildTarget.Android:
                  modifier = "android";
                  fileExtension = ".apk";
                  break;
               case BuildTarget.iOS:
                  modifier = "ios";
                  break;
               case BuildTarget.WSAPlayer:
                  modifier = "wsa";
                  break;
               case BuildTarget.WebGL:
                  modifier = "webgl";
                  break;
               case BuildTarget.tvOS:
                  modifier = "tvOS";
                  break;
               case BuildTarget.PS4:
                  modifier = "ps4";
                  break;
               case BuildTarget.XboxOne:
                  modifier = "xboxone";
                  break;
               case BuildTarget.Switch:
                  modifier = "switch";
                  break;
#if !UNITY_2018_2_OR_NEWER
               case BuildTarget.PSP2:
                  modifier = "psp2";
                  break;
               case BuildTarget.WiiU:
                  modifier = "wiiu";
                  break;
               case BuildTarget.N3DS:
                  modifier = "n3ds";
                  break;
#endif
               default:
                  Debug.LogError("Can't build, target not supported: " + target);
                  return false;
            }

            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);

            string buildPath = path + (Config.ADD_NAME_TO_PATH ? name + "_" : string.Empty) + modifier + (Config.ADD_DATE_TO_PATH ? System.DateTime.Now.ToString("_dd-MM-yyyy-HH-mm-ss") : string.Empty) + "/";
            string playerPath = buildPath + name + fileExtension;

            if (Config.DEBUG)
               Debug.Log("+++ BuildPlayer: '" + target.ToString() + "' at '" + buildPath + "' +++");

            if (deleteBuild)
            {
               try
               {
                  if (System.IO.Directory.Exists(buildPath))
                     System.IO.Directory.Delete(buildPath, true);
               }
               catch (System.Exception ex)
               {
                  Debug.LogError("Could not delete build path '" + buildPath + "': " + ex);
               }
            }

            BuildPipeline.BuildPlayer(Helper.ScenePaths, playerPath, target, (Config.BO_SHOW_BUILT_PLAYER ? BuildOptions.ShowBuiltPlayer : BuildOptions.None) | (Config.BO_DEVELOPMENT ? BuildOptions.Development : BuildOptions.None) | (Config.BO_PROFILER ? BuildOptions.ConnectWithProfiler : BuildOptions.None) | (Config.BO_SCRIPTDEBUG ? BuildOptions.AllowDebugging : BuildOptions.None));

            /*
            // ZIP everything
            CompressDirectory(buildPath, path + "/" + filename + modifier + ".zip");
            */

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_POST_BUILD))
               Helper.InvokeMethod(Config.EXECUTE_METHOD_POST_BUILD.Substring(0, Config.EXECUTE_METHOD_POST_BUILD.LastIndexOf(".")), Config.EXECUTE_METHOD_POST_BUILD.Substring(Config.EXECUTE_METHOD_POST_BUILD.LastIndexOf(".") + 1));

            CurrentBuildTarget = BuildTarget.NoTarget;

            success = true;
            //if (quit)
            //    EditorApplication.Exit(0);
         }
         else
         {
            Debug.LogWarning("No active scenes found - build not possible!");
         }

         return success;
      }

      /*
      // compress the folder into a ZIP file, uses https://github.com/r2d2rigo/dotnetzip-for-unity
      static void CompressDirectory(string directory, string zipFileOutputPath)
      {
          Debug.Log("attempting to compress " + directory + " into " + zipFileOutputPath);
          // display fake percentage, I can't get zip.SaveProgress event handler to work for some reason, whatever
          EditorUtility.DisplayProgressBar("COMPRESSING... please wait", zipFileOutputPath, 0.38f);
          using (ZipFile zip = new ZipFile())
          {
              zip.ParallelDeflateThreshold = -1; // DotNetZip bugfix that corrupts DLLs / binaries http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
              zip.AddDirectory(directory);
              zip.Save(zipFileOutputPath);
          }
          EditorUtility.ClearProgressBar();
      }
      */

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
using System.Linq;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.TPB.Util
{
   /// <summary>Various helper functions.</summary>
   public abstract class Helper : Common.EditorUtil.BaseEditorHelper
   {
      #region Static variables

      private static Texture2D logo_asset;
      private static Texture2D logo_asset_small;

      private static Texture2D icon_play;
      private static Texture2D icon_show;

      private static Texture2D logo_windows;
      private static Texture2D logo_mac;
      private static Texture2D logo_linux;
      private static Texture2D logo_ios;
      private static Texture2D logo_android;
      private static Texture2D logo_wsa;
      private static Texture2D logo_webgl;
      private static Texture2D logo_tvos;
      private static Texture2D logo_ps4;
      private static Texture2D logo_xboxone;
      private static Texture2D logo_switch;
#if !UNITY_2018_2_OR_NEWER
      private static Texture2D logo_wiiu;
      private static Texture2D logo_3ds;
      private static Texture2D logo_psp;
#endif

      private static Texture2D asset_rocktomate;

      private static string scanInfo;
      private static bool isScanning = false;
      public static bool isDeleting = false;

      #endregion


      #region Static properties

      public static Texture2D Logo_Asset
      {
         get { return loadImage(ref logo_asset, "logo_asset.png"); }
      }

      public static Texture2D Logo_Asset_Small
      {
         get { return loadImage(ref logo_asset_small, "logo_asset_small.png"); }
      }

      public static Texture2D Icon_Play
      {
         get { return loadImage(ref icon_play, "icon_play.png"); }
      }

      public static Texture2D Icon_Show
      {
         get { return loadImage(ref icon_show, "icon_show.png"); }
      }

      public static Texture2D Logo_Windows
      {
         get { return loadImage(ref logo_windows, "logo_windows.png"); }
      }

      public static Texture2D Logo_Mac
      {
         get { return loadImage(ref logo_mac, "logo_mac.png"); }
      }

      public static Texture2D Logo_Linux
      {
         get { return loadImage(ref logo_linux, "logo_linux.png"); }
      }

      public static Texture2D Logo_Ios
      {
         get { return loadImage(ref logo_ios, "logo_ios.png"); }
      }

      public static Texture2D Logo_Android
      {
         get { return loadImage(ref logo_android, "logo_android.png"); }
      }

      public static Texture2D Logo_Wsa
      {
         get { return loadImage(ref logo_wsa, "logo_wsa.png"); }
      }

      public static Texture2D Logo_Webgl
      {
         get { return loadImage(ref logo_webgl, "logo_webgl.png"); }
      }

      public static Texture2D Logo_Tvos
      {
         get { return loadImage(ref logo_tvos, "logo_tvos.png"); }
      }

      public static Texture2D Logo_Ps4
      {
         get { return loadImage(ref logo_ps4, "logo_ps4.png"); }
      }

      public static Texture2D Logo_Xboxone
      {
         get { return loadImage(ref logo_xboxone, "logo_xboxone.png"); }
      }

      public static Texture2D Logo_Switch
      {
         get { return loadImage(ref logo_switch, "logo_switch.png"); }
      }

#if !UNITY_2018_2_OR_NEWER
      public static Texture2D Logo_Wiiu
      {
         get { return loadImage(ref logo_wiiu, "logo_wiiu.png"); }
      }

      public static Texture2D Logo_3ds
      {
         get { return loadImage(ref logo_3ds, "logo_3ds.png"); }
      }

      public static Texture2D Logo_Psp
      {
         get { return loadImage(ref logo_psp, "logo_psp.png"); }
      }
#endif

      public static Texture2D Asset_RockTomate
      {
         get { return loadImage(ref asset_rocktomate, "asset_rocktomate.png"); }
      }

      /// <summary>Returns all active platforms.</summary>
      /// <returns>All active platforms.</returns>
      public static System.Collections.Generic.List<BuildTarget> Targets
      {
         get
         {
            System.Collections.Generic.List<BuildTarget> targets = new System.Collections.Generic.List<BuildTarget>();

            if (Config.PLATFORM_WINDOWS)
               targets.Add(TargetWindows);

            if (Config.PLATFORM_MAC)
               targets.Add(TargetMac);

            if (Config.PLATFORM_LINUX)
               targets.Add(TargetLinux);

            if (Config.PLATFORM_ANDROID) //TODO textures?
               targets.Add(BuildTarget.Android);

            if (Config.PLATFORM_IOS)
               targets.Add(BuildTarget.iOS);

            if (Config.PLATFORM_WSA)
               targets.Add(BuildTarget.WSAPlayer);

            if (Config.PLATFORM_WEBGL)
               targets.Add(BuildTarget.WebGL);

            if (Config.PLATFORM_TVOS)
               targets.Add(BuildTarget.tvOS);

            if (Config.PLATFORM_PS4)
               targets.Add(BuildTarget.PS4);

            if (Config.PLATFORM_XBOXONE)
               targets.Add(BuildTarget.XboxOne);

            if (Config.PLATFORM_SWITCH)
               targets.Add(BuildTarget.Switch);

#if !UNITY_2018_2_OR_NEWER
            if (Config.PLATFORM_PSP2)
               targets.Add(BuildTarget.PSP2);

            if (Config.PLATFORM_WIIU)
               targets.Add(BuildTarget.WiiU);

            if (Config.PLATFORM_3DS)
               targets.Add(BuildTarget.N3DS);
#endif

            return targets;
         }
      }

      /// <summary>Returns the active Windows platform.</summary>
      /// <returns>Active Windows platform.</returns>
      public static BuildTarget TargetWindows
      {
         get
         {
            if (Config.ARCH_WINDOWS == 0)
               return BuildTarget.StandaloneWindows;

            return BuildTarget.StandaloneWindows64;
         }
      }

      /// <summary>Returns the active macOS platform.</summary>
      /// <returns>Active macOS platform.</returns>
      public static BuildTarget TargetMac
      {
         get { return BuildTarget.StandaloneOSX; }
      }

      /// <summary>Returns the active Linux platform.</summary>
      /// <returns>Active Linux platform.</returns>
      public static BuildTarget TargetLinux
      {
         get
         {
            if (Config.ARCH_LINUX == 0)
            {
               return BuildTarget.StandaloneLinux;
            }
            else if (Config.ARCH_LINUX == 1)
            {
               return BuildTarget.StandaloneLinux64;
            }

            return BuildTarget.StandaloneLinuxUniversal;
         }
      }

      /// <summary>Checks if the user has selected any architecture platforms.</summary>
      /// <returns>True if the user has selected any architecture platforms.</returns>
      public static bool hasActiveArchitecturePlatforms
      {
         get { return Config.PLATFORM_WINDOWS || Config.PLATFORM_LINUX; }
      }

      /*
      /// <summary>Checks if the user has selected any texture platforms.</summary>
      /// <returns>True if the user has selected any texture platforms.</returns>
      public static bool hasActiveTexturePlatforms
      {
          get
          {
              return Config.PLATFORM_ANDROID;
          }
      }
      */

      /// <summary>All active scene paths of the project.</summary>
      /// <returns>All active scene paths of the project.</returns>
      public static string[] ScenePaths
      {
         get { return (from t in EditorBuildSettings.scenes where t.enabled select t.path).ToArray(); }
      }

      /// <summary>Checks if a project has any active scenes.</summary>
      /// <returns>True if a project has any active scenes.</returns>
      public static bool hasActiveScenes
      {
         get { return ScenePaths.Length > 0; }
      }

      /// <summary>Checks if a build for the project exists.</summary>
      /// <returns>True if a build for the project exists.</returns>
      public static bool hasBuild
      {
         get { return System.IO.Directory.Exists(Config.PATH_BUILD); }
      }

      /// <summary>Scans the build usage information.</summary>
      /// <returns>Build usage information.</returns>
      public static string BuildInfo
      {
         get
         {
            string result = Constants.TEXT_NO_BUILDS;

            if (hasBuild)
            {
               if (!string.IsNullOrEmpty(scanInfo))
               {
                  result = scanInfo;
               }
               else
               {
                  if (!isScanning)
                  {
                     isScanning = true;

                     if (System.IO.Directory.Exists(Config.PATH_BUILD))
                     {
                        System.Threading.Thread worker;
                        worker = isWindowsEditor ? new System.Threading.Thread(() => scanWindows(Config.PATH_BUILD, ref scanInfo)) : new System.Threading.Thread(() => scanUnix(Config.PATH_BUILD, ref scanInfo));
                        worker.Start();
                     }
                  }
                  else
                  {
                     result = "Scanning...";
                  }
               }
            }

            return result;
         }
      }

      #endregion


      #region Public static methods

      /// <summary>Setup the VCS before building.</summary>
      public static void SetupVCS()
      {
         if (!Config.CUSTOM_PATH_BUILD && Config.VCS != 0)
         {
            switch (Config.VCS)
            {
               case 1:
               {
                  // git
                  try
                  {
                     if (System.IO.File.Exists(Constants.APPLICATION_PATH + ".gitignore"))
                     {
                        string content = System.IO.File.ReadAllText(Constants.APPLICATION_PATH + ".gitignore");

                        if (!content.Contains(Constants.BUILD_DIRNAME + "/"))
                        {
                           System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".gitignore", content.TrimEnd() + System.Environment.NewLine + Constants.BUILD_DIRNAME + "/");
                        }
                     }
                     else
                     {
                        System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".gitignore", Constants.BUILD_DIRNAME + "/");
                     }
                  }
                  catch (System.Exception ex)
                  {
                     string errorMessage = "Could not add entry to .gitignore! Please add the entry '" + Constants.BUILD_DIRNAME + "/' manually." + System.Environment.NewLine + ex;
                     Debug.LogError(errorMessage);
                  }

                  break;
               }
               case 2:
               {
                  // svn
                  using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                  {
                     process.StartInfo.FileName = "svn";
                     process.StartInfo.Arguments = "propset svn: ignore " + Constants.BUILD_DIRNAME + ".";
                     process.StartInfo.WorkingDirectory = Constants.APPLICATION_PATH;
                     process.StartInfo.UseShellExecute = false;

                     try
                     {
                        process.Start();
                        process.WaitForExit(Constants.PROCESS_KILL_TIME);
                     }
                     catch (System.Exception ex)
                     {
                        string errorMessage = "Could not execute svn-ignore! Please do it manually in the console: 'svn propset svn:ignore " + Constants.BUILD_DIRNAME + ".'" + System.Environment.NewLine + ex;
                        Debug.LogError(errorMessage);
                     }
                  }

                  break;
               }
               case 3:
                  // mercurial
                  Debug.LogWarning("Mercurial currently not supported. Please add the following lines to your .hgignore: " + System.Environment.NewLine + "syntax: glob" + System.Environment.NewLine + Constants.BUILD_DIRNAME + "/**");
                  break;
               case 4:
               {
                  // collab
                  try
                  {
                     if (System.IO.File.Exists(Constants.APPLICATION_PATH + ".collabignore"))
                     {
                        string content = System.IO.File.ReadAllText(Constants.APPLICATION_PATH + ".collabignore");

                        if (!content.Contains(Constants.BUILD_DIRNAME + "/"))
                        {
                           System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".collabignore", content.TrimEnd() + System.Environment.NewLine + Constants.BUILD_DIRNAME + "/");
                        }
                     }
                     else
                     {
                        System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".collabignore", Constants.BUILD_DIRNAME + "/");
                     }
                  }
                  catch (System.Exception ex)
                  {
                     string errorMessage = "Could not add entry to .collabignore! Please add the entry '" + Constants.BUILD_DIRNAME + "/' manually." + System.Environment.NewLine + ex;
                     Debug.LogError(errorMessage);
                  }

                  break;
               }
               case 5:
               {
                  // PlasticSCM
                  try
                  {
                     if (System.IO.File.Exists(Constants.APPLICATION_PATH + "ignore.conf"))
                     {
                        string content = System.IO.File.ReadAllText(Constants.APPLICATION_PATH + "ignore.conf");

                        if (!content.Contains(Constants.BUILD_DIRNAME))
                        {
                           System.IO.File.WriteAllText(Constants.APPLICATION_PATH + "ignore.conf", content.TrimEnd() + System.Environment.NewLine + Constants.BUILD_DIRNAME);
                        }
                     }
                     else
                     {
                        System.IO.File.WriteAllText(Constants.APPLICATION_PATH + "ignore.conf", Constants.BUILD_DIRNAME);
                     }
                  }
                  catch (System.Exception ex)
                  {
                     string errorMessage = "Could not add entry to ignore.conf! Please add the entry '" + Constants.BUILD_DIRNAME + "' manually." + System.Environment.NewLine + ex;
                     Debug.LogError(errorMessage);
                  }

                  break;
               }
               default:
               {
                  Debug.LogWarning("Unknown VCS selected: " + Config.VCS);
                  break;
               }
            }
         }
      }

      /// <summary>Delete the builds for all platforms.</summary>
      public static void DeleteBuilds()
      {
         if (!isDeleting && System.IO.Directory.Exists(Config.PATH_BUILD))
         {
            isDeleting = true;

            System.Threading.Thread worker = new System.Threading.Thread(() => deleteBuilds());
            worker.Start();
         }
      }

#if CT_TPS
        /// <summary>Builds the target.</summary>
        /// <param name="target">Target platform for the build</param>
        /// <param name="batchmode">Build in batch-mode (default: true, optional)</param>
        public static void ProcessBuildPipeline(string target, bool batchmode = true)
        {
            bool success = false;

            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                try
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;

                    string scriptfile;
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TPB-" + System.Guid.NewGuid() + ".cmd";

                        System.IO.File.WriteAllText(scriptfile, generateWindowsScript(target, batchmode));

                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = "/c start  \"\" " + '"' + scriptfile + '"';
                    }
                    else if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TPB-" + System.Guid.NewGuid() + ".sh";

                        System.IO.File.WriteAllText(scriptfile, generateMacScript(target, batchmode));

                        process.StartInfo.FileName = "/bin/sh";
                        process.StartInfo.Arguments = '"' + scriptfile + "\" &";
                    }
                    else if (Application.platform == RuntimePlatform.LinuxEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TPB-" + System.Guid.NewGuid() + ".sh";

                        System.IO.File.WriteAllText(scriptfile, generateLinuxScript(target, batchmode));

                        process.StartInfo.FileName = "/bin/sh";
                        process.StartInfo.Arguments = '"' + scriptfile + "\" &";
                    }
                    else
                    {
                        Debug.LogError("Unsupported Unity Editor: " + Application.platform);
                        return;
                    }

                    Config.Save();

                    process.Start();

                    if (isWindowsEditor)
                        process.WaitForExit(Constants.PROCESS_KILL_TIME);

                    success = true;
                }
                catch (System.Exception ex)
                {
                    string errorMessage = "Could execute TPB!" + System.Environment.NewLine + ex;
                    Debug.LogError(errorMessage);
                }
            }

            if (success)
                EditorApplication.Exit(0);
        }
#endif

      #endregion


      #region Private static methods

      private static void scanWindows(string path, ref string key)
      {
         using (System.Diagnostics.Process scanProcess = new System.Diagnostics.Process())
         {
            string args = "/c dir * /s /a";

            if (Config.DEBUG)
               Debug.Log("Process arguments: '" + args + "'");

            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

            scanProcess.StartInfo.FileName = "cmd.exe";
            scanProcess.StartInfo.WorkingDirectory = path;
            scanProcess.StartInfo.Arguments = args;
            scanProcess.StartInfo.CreateNoWindow = true;
            scanProcess.StartInfo.RedirectStandardOutput = true;
            scanProcess.StartInfo.RedirectStandardError = true;
            scanProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.OutputDataReceived += (sender, eventArgs) => { result.Add(eventArgs.Data); };

            bool success = true;

            try
            {
               scanProcess.Start();
               scanProcess.BeginOutputReadLine();
            }
            catch (System.Exception ex)
            {
               success = false;
               Debug.LogError("Could not start the scan process!" + System.Environment.NewLine + ex);
            }

            if (success)
            {
               do
               {
                  System.Threading.Thread.Sleep(50);
               } while (!scanProcess.HasExited);

               if (scanProcess.ExitCode == 0)
               {
                  if (Config.DEBUG)
                     Debug.LogWarning("Scan completed: " + result.Count);

                  key = result[result.Count - 3].Trim();
               }
               else
               {
                  using (System.IO.StreamReader sr = scanProcess.StandardError)
                  {
                     Debug.LogError("Could not scan the path: " + scanProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd());
                  }
               }
            }
         }
      }

      private static void scanUnix(string path, ref string key)
      {
         using (System.Diagnostics.Process scanProcess = new System.Diagnostics.Process())
         {
            string args = "-sch \"" + path + '"';

            if (Config.DEBUG)
               Debug.Log("Process arguments: '" + args + "'");

            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

            scanProcess.StartInfo.FileName = "du";
            scanProcess.StartInfo.Arguments = args;
            scanProcess.StartInfo.CreateNoWindow = true;
            scanProcess.StartInfo.RedirectStandardOutput = true;
            scanProcess.StartInfo.RedirectStandardError = true;
            scanProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.OutputDataReceived += (sender, eventArgs) => { result.Add(eventArgs.Data); };

            bool success = true;

            try
            {
               scanProcess.Start();
               scanProcess.BeginOutputReadLine();
            }
            catch (System.Exception ex)
            {
               success = false;
               Debug.LogError("Could not start the scan process!" + System.Environment.NewLine + ex);
            }

            if (success)
            {
               while (!scanProcess.HasExited)
               {
                  System.Threading.Thread.Sleep(50);
               }

               if (scanProcess.ExitCode == 0)
               {
                  if (Config.DEBUG)
                     Debug.LogWarning("Scan completed: " + result.Count);

                  key = result[result.Count - 2].Trim();
               }
               else
               {
                  using (System.IO.StreamReader sr = scanProcess.StandardError)
                  {
                     Debug.LogError("Could not scan the path: " + scanProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd());
                  }
               }
            }
         }
      }

      private static void deleteBuilds()
      {
         try
         {
            System.IO.Directory.Delete(Config.PATH_BUILD, true);
         }
         catch (System.Exception ex)
         {
            Debug.LogWarning("Could not delete the builds!" + System.Environment.NewLine + ex);
         }

         isDeleting = false;
      }

#if CT_TPS
        #region Windows

        private static string generateWindowsScript(string target, bool batchmode)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("@echo off");
            sb.AppendLine("cls");

            // title
            sb.Append("title ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Building of ");
            sb.Append(Application.productName);
            sb.Append(" for ");
            sb.Append(target);
            sb.AppendLine(" - DO NOT CLOSE THIS WINDOW!");

            // header
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #                                                                            #");
            sb.Append("echo #  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - Windows                                      #");
            sb.AppendLine("echo #  Copyright 2018-2020 by www.crosstales.com                                 #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo #  The build for the platform has started.                                   #");
            sb.AppendLine("echo #  This will take some time, so please be patient and DON'T CLOSE THIS       #");
            sb.AppendLine("echo #  WINDOW before the process is finished!                                    #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo #  Unity will restart automatically after the build.                         #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo " + Application.productName);
            sb.AppendLine("echo.");
            sb.AppendLine("echo.");

            // check if Unity is closed
            sb.AppendLine(":waitloop");
            sb.Append("if not exist \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp\\UnityLockfile\" goto waitloopend");
            sb.AppendLine();
            sb.AppendLine("echo.");
            sb.AppendLine("echo Waiting for Unity to close...");
            sb.AppendLine("timeout /t 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("del \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp\\UnityLockfile\" /q");
                sb.AppendLine();
            }

            sb.AppendLine("goto waitloop");
            sb.AppendLine(":waitloopend");

            //  Launching TPS
            sb.AppendLine("echo.");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #  Launching TPS                                                             #");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine();
            sb.Append("start \"\" \"");
            sb.Append(Helper.ValidatePath(EditorApplication.applicationPath, false));
            sb.Append("\" -projectPath \"");
            sb.Append(Constants.PATH.Substring(0, Constants.PATH.Length - 1));
            sb.Append("\"");
            sb.Append(" -batchmode");
            sb.Append(" -executeMethod Crosstales.TPS.Switcher.SwitchCLI");
            sb.Append(" -tpsExecuteMethod Crosstales.TPB.Builder.BuildTPS");
            if (batchmode)
                sb.Append(" -tpsBatchmode true");
            sb.Append(" -tpsBuild ");
            sb.Append(target);

            sb.AppendLine();
            sb.AppendLine("echo.");

            // check if Unity is started
            sb.AppendLine(":waitloop2");
            sb.Append("if exist \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp\\UnityLockfile\" goto waitloopend2");
            sb.AppendLine();
            sb.AppendLine("echo Waiting for Unity to start...");
            sb.AppendLine("timeout /t 3");
            sb.AppendLine("goto waitloop2");
            sb.AppendLine(":waitloopend2");
            sb.AppendLine("echo.");
            sb.AppendLine("echo Bye!");
            sb.AppendLine("timeout /t 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        #endregion


        #region Mac

        private static string generateMacScript(string target, bool batchmode)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine("set +v");
            sb.AppendLine("clear");

            // title
            sb.Append("title='");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Building of ");
            sb.Append(Application.productName);
            sb.Append(" for ");
            sb.Append(target);
            sb.AppendLine(" - DO NOT CLOSE THIS WINDOW!'");
            sb.AppendLine("echo -n -e \"\\033]0;$title\\007\"");

            // header
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.Append("echo \"¦  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - macOS                                        ¦\"");
            sb.AppendLine("echo \"¦  Copyright 2018-2020 by www.crosstales.com                                 ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  The build for the platform has started.                                   ¦\"");
            sb.AppendLine("echo \"¦  This will take some time, so please be patient and DON'T CLOSE THIS       ¦\"");
            sb.AppendLine("echo \"¦  WINDOW before the process is finished!                                    ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  Unity will restart automatically after the build.                         ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"" + Application.productName + "\"");
            sb.AppendLine("echo");
            sb.AppendLine("echo");

            // check if Unity is closed
            sb.Append("while [ -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to close...\"");
            sb.AppendLine("  sleep 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("  rm \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp/UnityLockfile\"");
                sb.AppendLine();
            }

            sb.AppendLine("done");

            // Launching TPS
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦ Launching TPS                                                              ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine();
            sb.Append("open -a \"");
            sb.Append(EditorApplication.applicationPath);
            sb.Append("\" --args -projectPath \"");
            sb.Append(Constants.PATH);
            sb.Append("\"");
            sb.Append(" -batchmode");
            sb.Append(" -executeMethod Crosstales.TPS.Switcher.SwitchCLI");
            sb.Append(" -tpsExecuteMethod Crosstales.TPB.Builder.BuildTPS");
            if (batchmode)
                sb.Append(" -tpsBatchmode true");
            sb.Append(" -tpsBuild ");
            sb.Append(target);

            sb.AppendLine();

            //check if Unity is started
            sb.AppendLine("echo");
            sb.Append("while [ ! -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to start...\"");
            sb.AppendLine("  sleep 3");
            sb.AppendLine("done");
            sb.AppendLine("echo");
            sb.AppendLine("echo \"Bye!\"");
            sb.AppendLine("sleep 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        #endregion


        #region Linux

        private static string generateLinuxScript(string target, bool batchmode)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine("set +v");
            sb.AppendLine("clear");

            // title
            sb.Append("title='");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Build of ");
            sb.Append(Application.productName);
            sb.Append(" for ");
            sb.Append(target);
            sb.AppendLine(" - DO NOT CLOSE THIS WINDOW!'");
            sb.AppendLine("echo -n -e \"\\033]0;$title\\007\"");

            // header
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.Append("echo \"¦  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - Linux                                        ¦\"");
            sb.AppendLine("echo \"¦  Copyright 2018-2020 by www.crosstales.com                                 ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  The build for the platform has started.                                   ¦\"");
            sb.AppendLine("echo \"¦  This will take some time, so please be patient and DON'T CLOSE THIS       ¦\"");
            sb.AppendLine("echo \"¦  WINDOW before the process is finished!                                    ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  Unity will restart automatically after the build.                         ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"" + Application.productName + "\"");
            sb.AppendLine("echo");
            sb.AppendLine("echo");

            // check if Unity is closed
            sb.Append("while [ -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to close...\"");
            sb.AppendLine("  sleep 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("  rm \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp/UnityLockfile\"");
                sb.AppendLine();
            }

            sb.AppendLine("done");

            // Launching TPS
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Launching TPS                                                             ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.Append('"');
            sb.Append(EditorApplication.applicationPath);
            sb.Append("\" --args -projectPath \"");
            sb.Append(Constants.PATH);
            sb.Append("\"");
            sb.Append(" -batchmode");
            sb.Append(" -executeMethod Crosstales.TPS.Switcher.SwitchCLI");
            sb.Append(" -tpsExecuteMethod Crosstales.TPB.Builder.BuildTPS");
            if (batchmode)
                sb.Append(" -tpsBatchmode true");
            sb.Append(" -tpsBuild ");
            sb.Append(target);

            sb.Append(" &");
            sb.AppendLine();

            // check if Unity is started
            sb.AppendLine("echo");
            sb.Append("while [ ! -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to start...\"");
            sb.AppendLine("  sleep 3");
            sb.AppendLine("done");
            sb.AppendLine("echo");
            sb.AppendLine("echo \"Bye!\"");
            sb.AppendLine("sleep 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        #endregion
#endif

      private static Texture2D loadImage(ref Texture2D logo, string fileName)
      {
         if (logo == null)
         {
#if CT_DEVELOP
            logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets" + Config.ASSET_PATH + "Icons/" + fileName, typeof(Texture2D));
#else
                logo = (Texture2D)EditorGUIUtility.Load("crosstales/TurboBuilder/" + fileName);
#endif

            if (logo == null)
            {
               Debug.LogWarning("Image not found: " + fileName);
            }
         }

         return logo;
      }

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
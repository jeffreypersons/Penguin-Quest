#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.TPS
{
   /// <summary>Platform switcher.</summary>
   public static class Switcher
   {
      /// <summary>The current switch target.</summary>
      public static BuildTarget CurrentSwitchTarget = BuildTarget.NoTarget;

      /// <summary>Switches the current platform to the target via CLI.</summary>
      public static void SwitchCLI()
      {
         Switch(Util.Helper.getCLIArgument("-tpsBuild"), Util.Helper.getCLIArgument("-tpsExecuteMethod"), "true".CTEquals(Util.Helper.getCLIArgument("-tpsBatchmode")), !"false".CTEquals(Util.Helper.getCLIArgument("-tpsQuit")), "true".CTEquals(Util.Helper.getCLIArgument("-tpsNoGraphics")), "true".CTEquals(Util.Helper.getCLIArgument("-tpsCopySettings")));
      }

      /// <summary>Switches the current platform to the target.</summary>
      /// <param name="build">Build type name for Unity, like 'win64'</param>
      /// <param name="executeMethod">Execute method after switch (optional)</param>
      /// <param name="batchmode">Start Unity in batch-mode (default: false, optional)</param>
      /// <param name="quit">Quit Unity in batch-mode (default: true, optional)</param>
      /// <param name="noGraphics">Disable graphic devices in batch-mode (default: false, optional)</param>
      /// <param name="copySettings">Copy the project settings (default: false, optional)</param>
      /// <returns>True if the switch was successful.</returns>
      public static bool Switch(string build, string executeMethod = "", bool batchmode = false, bool quit = true, bool noGraphics = false, bool copySettings = false)
      {
         Util.Config.EXECUTE_METHOD = executeMethod;
         Util.Config.BATCHMODE = batchmode;
         Util.Config.QUIT = quit;
         Util.Config.NO_GRAPHICS = noGraphics;
         Util.Config.COPY_SETTINGS = copySettings;

         return Switch(Util.Helper.getBuildTargetForBuildName(build));
      }

      /// <summary>Switches the current platform to the target.</summary>
      /// <param name="target">Target platform for the switch</param>
      /// <param name="subTarget">Texture format (Android, optional)</param>
      /// <returns>True if the switch was successful.</returns>
      public static bool Switch(BuildTarget target, MobileTextureSubtarget subTarget = MobileTextureSubtarget.Generic)
      {
         CurrentSwitchTarget = target;
         bool success = false;

         if (target == EditorUserBuildSettings.activeBuildTarget) //ignore switch
         {
            Debug.LogWarning("Target platform is equals the current platform - switch ignored.");

            if (!string.IsNullOrEmpty(Util.Config.EXECUTE_METHOD_PRE_SWITCH))
               Common.EditorUtil.BaseEditorHelper.InvokeMethod(Util.Config.EXECUTE_METHOD_PRE_SWITCH.Substring(0, Util.Config.EXECUTE_METHOD_PRE_SWITCH.LastIndexOf(".")), Util.Config.EXECUTE_METHOD_PRE_SWITCH.Substring(Crosstales.TPS.Util.Config.EXECUTE_METHOD_PRE_SWITCH.LastIndexOf(".") + 1));

            if (!string.IsNullOrEmpty(Util.Config.EXECUTE_METHOD))
               Common.EditorUtil.BaseEditorHelper.InvokeMethod(Util.Config.EXECUTE_METHOD.Substring(0, Util.Config.EXECUTE_METHOD.LastIndexOf(".")), Util.Config.EXECUTE_METHOD.Substring(Util.Config.EXECUTE_METHOD.LastIndexOf(".") + 1));

            success = true;
         }
         else
         {
            if (Util.Config.USE_LEGACY)
            {
               success = Util.Helper.SwitchPlatform(target, subTarget);
            }
            else
            {
               success = Util.Helper.SwitchPlatformNew(target, subTarget);
            }
         }

         CurrentSwitchTarget = BuildTarget.NoTarget;

#if UNITY_2018_2_OR_NEWER
         if (Application.isBatchMode && !Util.Config.USE_LEGACY && Util.Config.QUIT)
#else
         if (UnityEditorInternal.InternalEditorUtility.inBatchMode && !Util.Config.USE_LEGACY && Util.Config.QUIT)
#endif
         {
            EditorApplication.Exit(0);
         }

         return success;
      }

      /// <summary>Test switching with an execute method.</summary>
      public static void SayHello()
      {
         Debug.LogWarning("Hello everybody, 'SayHello' was called!");

         if (Util.Config.DEBUG)
            Debug.Log("CurrentSwitchTarget: " + CurrentSwitchTarget);
      }

      /// <summary>Test method (before switching).</summary>
      public static void MethodBeforeSwitch()
      {
         Debug.LogWarning("'MethodBeforeSwitch' was called!");
      }

      /// <summary>Test method (after switching).</summary>
      public static void MethodAfterSwitch()
      {
         Debug.LogWarning("'MethodAfterSwitch' was called");
      }
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
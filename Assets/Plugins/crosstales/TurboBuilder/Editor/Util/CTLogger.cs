#if UNITY_EDITOR
namespace Crosstales.TPB.Util
{
   /// <summary>Logger for the asset.</summary>
   public static class CTLogger
   {
      private static string fileMethods = System.IO.Path.GetTempPath() + "TPB_Methods.log";
      private static string fileLog = System.IO.Path.GetTempPath() + "TPB.log";

      public static void Log(string log)
      {
         System.IO.File.AppendAllText(fileLog, System.DateTime.Now.ToLocalTime() + " - " + log + System.Environment.NewLine);
      }

      public static void BeforeBuild()
      {
         System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - BeforeBuild: " + Builder.CurrentBuildTarget + System.Environment.NewLine);
      }

      public static void AfterBuild()
      {
         System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - AfterBuild: " + Builder.CurrentBuildTarget + System.Environment.NewLine);
      }

      public static void BeforeBuilding()
      {
         System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - +++ BeforeBuilding +++" + System.Environment.NewLine);
      }

      public static void AfterBuilding()
      {
         System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - +++ AfterBuilding +++" + System.Environment.NewLine);
      }
   }
}
#endif
// © 2019 crosstales LLC (https://www.crosstales.com)
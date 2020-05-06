#if UNITY_EDITOR
namespace Crosstales.TPS.Util
{
   /// <summary>Logger for the asset.</summary>
   public static class CTLogger
   {
      private static readonly string fileMethods = System.IO.Path.GetTempPath() + "TPS_Methods.log";
      private static readonly string fileLog = System.IO.Path.GetTempPath() + "TPS.log";

      public static void Log(string log)
      {
         System.IO.File.AppendAllText(fileLog, System.DateTime.Now.ToLocalTime() + " - " + log + System.Environment.NewLine);
      }

      public static void BeforeSwitch()
      {
         System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - BeforeSwitch: " + Switcher.CurrentSwitchTarget + System.Environment.NewLine);
      }

      public static void AfterSwitch()
      {
         System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - AfterSwitch" + System.Environment.NewLine);
      }
   }
}
#endif
// © 2019 crosstales LLC (https://www.crosstales.com)
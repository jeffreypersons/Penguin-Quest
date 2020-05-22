#if UNITY_EDITOR
using UnityEditor;

namespace Crosstales.TPS.EditorTask
{
   /// <summary>Adds the given define symbols to PlayerSettings define symbols.</summary>
   [InitializeOnLoad]
   public class CompileDefines : Common.EditorTask.BaseCompileDefines
   {
      private const string symbol = "CT_TPS";

      static CompileDefines()
      {
         if (Util.Config.COMPILE_DEFINES)
            addSymbolsToAllTargets(symbol);
      }
   }
}
#endif
// © 2017-2020 crosstales LLC (https://www.crosstales.com)
#if UNITY_EDITOR
using UnityEditor;

namespace Crosstales.TPB.EditorTask
{
   /// <summary>Adds the given define symbols to PlayerSettings define symbols.</summary>
   [InitializeOnLoad]
   public class CompileDefines : Common.EditorTask.BaseCompileDefines
   {
      private const string symbol = "CT_TPB";

      static CompileDefines()
      {
         addSymbolsToAllTargets(symbol);
      }
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)
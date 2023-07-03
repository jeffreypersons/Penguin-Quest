using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;


namespace PQ.Tests.EditMode
{
    // stub for unit tests in editor (eg unit tests for non-unity specific code such as custom data structures, inventory system)
    public class EditModeTestsStub
    {
        [Test]
        public void EditModeTestsStubSimplePasses()
        {
            // simple assertions go here ie `Assert(shouldBeTrue, messageIfNotTrue);`
        }

        [UnityTest]
        public IEnumerator EditModeTestsStubWithEnumeratorPasses()
        {
            // like coroutines in play mode, `yield return null;` can be used to skip a frame in edit mode
            yield return null;
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;


namespace PQ.Tests.PlayMode
{
    // stub for tests in Unity player (eg unit/integration-tests for unity specific code such as character controller, rendering)
    public class ScenarioTests
    {
        [Test]
        public void PlayModeTestStubSimplePasses()
        {
            // simple assertions go here ie `Assert(shouldBeTrue, messageIfNotTrue);`
        }

        [UnityTest]
        public IEnumerator PlayModeTestStubWithEnumeratorPasses()
        {
            // like coroutines in play mode, `yield return null;` can be used to skip a frame in edit mode
            yield return null;
        }
    }
}

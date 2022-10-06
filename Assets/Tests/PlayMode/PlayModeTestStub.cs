using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;


public class PlayModeTestStub
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

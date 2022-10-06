using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;


// stub for any tests that don't requiring playing a scene
public class EditTests
{
    [Test]
    public void EditTestsSimplePasses()
    {
        // simple assertions go here ie `Assert(shouldBeTrue, messageIfNotTrue);`
    }

    [UnityTest]
    public IEnumerator EditTestsWithEnumeratorPasses()
    {
        // like coroutines in play mode, `yield return null;` can be used to skip a frame in edit mode
        yield return null;
    }
}

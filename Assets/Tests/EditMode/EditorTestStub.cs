using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;


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

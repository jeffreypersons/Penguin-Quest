using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
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
}

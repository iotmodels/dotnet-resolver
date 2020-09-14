using IoTModels.Resolvers;
using NUnit.Framework;

namespace resolver_tests
{
    public class DtmiConventionFixture
    {
        [Test]
        public void DtmiToPath()
        {
            Assert.AreEqual("dtmi/com/example/model-1.json", DtmiConvention.Dtmi2Path("dtmi:com:Example:Model;1"));
            Assert.AreEqual("dtmi/com/example/model-1.json", DtmiConvention.Dtmi2Path("dtmi:com:example:Model;1"));
        }
    }
}
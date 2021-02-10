using System;
using System.IO;
using System.Xml.Schema;
using NUnit.Framework;

namespace Gehtsoft.Build.Nuget.Test
{
    [TestFixture]
    public class TestValidation
    {
        [Test]
        public void LoadSchema()
        {
            Assert.IsNotNull(NugetSpecSchemaProvider.GetSchema());

        }

        [Test]
        public void TestCorrect()
        {
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.MinimumCorrectSample.xml"))
            {
                Assert.IsNotNull(new NugetConfigFile(stream));
            }
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.FullCorrectSample.xml"))
            {
                Assert.IsNotNull(new NugetConfigFile(stream));
            }
        }


        [Test]
        public void TestIncorrect()
        {
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.IncorrectSample1.xml"))
            {
                Assert.Throws<XmlSchemaValidationException>(() => new NugetConfigFile(stream));
            }
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.IncorrectSample2.xml"))
            {
                Assert.Throws<XmlSchemaValidationException>(() => new NugetConfigFile(stream));
            }
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.IncorrectSample3.xml"))
            {
                Assert.Throws<XmlSchemaValidationException>(() => new NugetConfigFile(stream));
            }
        }
    }


}

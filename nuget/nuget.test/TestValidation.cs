using System;
using System.IO;
using System.Xml.Schema;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gehtsoft.Build.Nuget.Test
{
    [TestFixture]
    public class TestValidation
    {
        [Test]
        public void LoadSchema()
        {
            ClassicAssert.IsNotNull(NugetSpecSchemaProvider.GetSchema());

        }

        [TestCase("MinimumCorrectSample")]
        [TestCase("FullCorrectSample")]
        public void TestCorrect(string resource)
        {
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.{resource}.xml"))
            {
                Action action = () => new NugetConfigFile(stream);
                action.Should().NotThrow();
            }
        }


        [TestCase("IncorrectSample1")]
        [TestCase("IncorrectSample2")]
        [TestCase("IncorrectSample3")]
        public void TestIncorrect(string resource)
        {
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.{resource}.xml"))
            {
                Action action = () => new NugetConfigFile(stream);
                action.Should().Throw<XmlSchemaValidationException>();
            }
        }
    }


}

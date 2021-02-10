using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Gehtsoft.Build.Nuget.Test
{
    [TestFixture]
    public class TestGeneration
    {
        [Test]
        public void Test1()
        {
            NugetConfigFile config;
            CSProjFile proj;
            GenerateNuspec task = new GenerateNuspec();

            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.Gen1.xml"))
            {
                config = new NugetConfigFile(stream);
            }
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.Gen1.csproj"))
            {
                proj = new CSProjFile(stream);
            }

            var spec = task.HandleProject(config.Projects[0], proj, new FileInfo(typeof(TestValidation).Assembly.Location), config);
            ;
        }
        [Test]
        public void Test2()
        {
            NugetConfigFile config;
            CSProjFile proj;
            GenerateNuspec task = new GenerateNuspec();

            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.Gen2.xml"))
            {
                config = new NugetConfigFile(stream);
            }
            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.Gen2.csproj"))
            {
                proj = new CSProjFile(stream);
            }

            var spec = task.HandleProject(config.Projects[0], proj, new FileInfo(typeof(TestValidation).Assembly.Location), config);
            ;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Gehtsoft.Build.Nuget.Test
{
    [TestFixture]
    public class TestGeneration
    {
        [TestCase("Gen1")]
        [TestCase("Gen2")]
        public void Test1(string resource)
        {
            NugetConfigFile config;
            CSProjFile proj;
            GenerateNuspec task = new GenerateNuspec();

            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.{resource}.xml"))
            {
                config = new NugetConfigFile(stream);
            }

            using (Stream stream = typeof(TestValidation).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.Test.res.{resource}.csproj"))
            {
                proj = new CSProjFile(stream);
            }

            NugetSpecificationFile spec; 
            Action action = () => spec = task.HandleProject(config.Projects[0], proj, new FileInfo(typeof(TestValidation).Assembly.Location), config);
            action.Should().NotThrow();
            ;
        }
    }
}

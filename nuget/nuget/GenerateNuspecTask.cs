using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gehtsoft.Build.Nuget
{
    public class GenerateNuspec : Microsoft.Build.Utilities.Task
    {
        //The location of nuget configuration
        public string NugetConfig { get; set; }

        //The base path to search projects
        public string BasePath { get; set; } = "..";

        public string OutputPath { get; set; } = ".";

        public override bool Execute()
        {
            if (!File.Exists(NugetConfig))
                throw new Exception("Configuration file is not found");

            NugetConfigFile config = new NugetConfigFile(NugetConfig);
            foreach (NugetConfigFile.Project projectSpec in config.Projects)
            {
                string projectId = projectSpec.Id;
                string projectPath;
                if (!string.IsNullOrEmpty(projectSpec.Location))
                    projectPath = Path.GetFullPath(projectSpec.Location);
                else
                    projectPath = Path.GetFullPath(Path.Combine(BasePath, projectId, projectId + ".csproj"));
                
                CSProjFile project = new CSProjFile(projectPath);
                FileInfo projectLocation = new FileInfo(projectPath);
                var nuspec = HandleProject(projectSpec, project, projectLocation, config);
                var nuspecFile = Path.Combine(OutputPath, $"{projectSpec.Id}.nuspec");
                nuspec.Save(nuspecFile);
            }
            return true;
        }

        public NugetSpecificationFile HandleProject(NugetConfigFile.Project projectSpec, CSProjFile project, FileInfo projectLocation, NugetConfigFile config)
        {
            var version = config.Versions.Last;
            var versionId = version.Id;
            if (projectSpec.HasSubVersion)
                versionId += "." + projectSpec.SubVersion;

            NugetSpecificationFile nuspec = new NugetSpecificationFile();
            
            //prepare project spec
            nuspec.Id = projectSpec.Id;
            nuspec.Version = versionId;
            nuspec.Authors = config.Owner;
            nuspec.Owners = config.Owner;
            if (config.License != null)
                nuspec.SetLicense(config.License.Type, config.License.Value);

            if (!string.IsNullOrEmpty(config.LicenseUrl))
                nuspec.LicenseUrl = config.LicenseUrl;
            nuspec.ProjectUrl = config.ProjectUrl;
            nuspec.RequireLicenseAcceptance = false;
            nuspec.Description = projectSpec.Description;
            nuspec.ReleaseNotes = version.Description;

            foreach (NugetConfigFile.Property prop in config.Properties)
                nuspec[prop.Id] = prop.Value;

            foreach (NugetConfigFile.Property prop in projectSpec.Properties)
                nuspec[prop.Id] = prop.Value;

            //prepare dependencies
            string[] targetFrameworks = project.TargetFrameworks;
            var packageReferences = project.FindNugetReferences();
            foreach (var packageReference in packageReferences)
            {
                if (config.ExcludeReference.Contains(packageReference.Id))
                    continue;

                if (targetFrameworks.Length == 1)
                    nuspec.AddDependency(packageReference.Id, packageReference.Version);
                else
                {
                    if (string.IsNullOrEmpty(packageReference.TargetFramework))
                    {
                        foreach (string targetFramework in targetFrameworks)
                            nuspec.AddDependency(packageReference.Id, packageReference.Version, targetFramework);
                    }
                    else
                        nuspec.AddDependency(packageReference.Id, packageReference.Version, packageReference.TargetFramework);
                }
            }

            var projectReferences = project.FindProjectReferences();
            foreach (var projectReference in projectReferences)
            {
                var referencePath = projectReference.ProjectPath;
                if (!Path.IsPathRooted(referencePath))
                    referencePath = Path.GetFullPath(Path.Combine(projectLocation.DirectoryName, referencePath));

                bool found = false;
                //try to find project in our spec
                foreach (var otherProject in config.Projects)
                {
                    string projectId = otherProject.Id;
                    string projectPath;
                    if (!string.IsNullOrEmpty(otherProject.Location))
                        projectPath = Path.GetFullPath(otherProject.Location);
                    else
                        projectPath = Path.GetFullPath(Path.Combine(BasePath, projectId, projectId + ".csproj"));
                    if (string.Compare(projectPath, referencePath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        found = true;
                        string otherVersion = version.Id;
                        if (otherProject.HasSubVersion)
                            otherVersion += "." + otherProject.SubVersion;

                        if (targetFrameworks.Length == 1)
                            nuspec.AddDependency(otherProject.Id, otherVersion);
                        else
                        {
                            foreach (string targetFramework in targetFrameworks)
                                nuspec.AddDependency(otherProject.Id, otherVersion, targetFramework);
                        }
                        break;
                    }
                }

                if (!found)
                    throw new Exception("The reference project is not a part of current nuget specification");
            }

            foreach (string targetFramework in targetFrameworks)
                nuspec.AddFile(Path.Combine(project.Location, $"bin/Release/{targetFramework}/{projectSpec.Id}.dll"), $"{projectSpec.TargetFolder}/{targetFramework}");

            foreach (var additionalFile in projectSpec.AdditionalFiles)
            {
                string source = Path.Combine(project.Location, additionalFile.File);
                if (additionalFile.Target == "*")
                {
                    foreach (string targetFramework in targetFrameworks)
                        nuspec.AddFile(source, $"{projectSpec.TargetFolder}/{targetFramework}");
                }
                else
                    nuspec.AddFile(source, additionalFile.Target);
            }

            return nuspec;
        }
    }
}

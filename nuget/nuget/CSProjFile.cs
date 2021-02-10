using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Gehtsoft.Build.Nuget
{
    public class CSProjFile
    {
        private readonly XmlDocument mDocument;
        private readonly XmlElement mRoot;
        public FileInfo mFileInfo;

        public string Location => mFileInfo.DirectoryName;

        public CSProjFile(string file)
        {
            mFileInfo = new FileInfo(Path.GetFullPath(file));

            if (!mFileInfo.Exists)
                throw new Exception($"Project file {file} is not found");

            mDocument = new XmlDocument();
            mDocument.LoadFile(file);
            mRoot = mDocument.DocumentElement;
        }
        public CSProjFile(Stream fs)
        {
            mFileInfo = new FileInfo("./file.csproj");
            mDocument = new XmlDocument();
            mDocument.LoadFile(fs);
            mRoot = mDocument.DocumentElement;
        }

        public string[] TargetFrameworks
        {
            get
            {
                XmlElement el = mRoot.Children("PropertyGroup").Children("TargetFramework").FirstOrDefault();
                if (el != null)
                    return new string[] {el.InnerText};
                el = mRoot.Children("PropertyGroup").Children("TargetFrameworks").FirstOrDefault();
                if (el != null)
                    return el.InnerText.Split(new char[]{ ';' }, StringSplitOptions.RemoveEmptyEntries);
                return null;
            }
        }

        public class NugetReference
        {
            public string Id { get; }
            public string TargetFramework { get; }
            public string Version { get; }
            public string IncludeAssets { get; set; }
            public string ExcludeAssets { get; set; }
            public string PrivateAssets { get; set; }

            public NugetReference(string id, string targetFramework, string version)
            {
                Id = id;
                TargetFramework = targetFramework;
                Version = version;
            }
        }

        Regex mConditionParser = new Regex(@"^\s*'\$\(TargetFramework\)'\s*==\s*'([^']+)'\s*$");

        public IReadOnlyList<NugetReference> FindNugetReferences()
        {
            List<NugetReference> refs = new List<NugetReference>();

            var elements = mRoot.Children("ItemGroup").Children("PackageReference");
            foreach (var element in elements)
            {
                string id = element.Attribute("Include");
                string version = element.AttributeOrElement("Version");
                string targetFramework = null;

                XmlElement parent = element.ParentNode as XmlElement;
                if (parent != null && parent.HasAttribute("Condition"))
                {
                    //need to parse condition
                    string condition = parent.Attribute("Condition");
                    Match m = mConditionParser.Match(condition);
                    if (!m.Success)
                        throw new Exception("The condition of a property group that consists of a package reference is in incompatible format");
                    targetFramework = m.Groups[1].Value;
                }

                var reference = new NugetReference(id, targetFramework, version);
                reference.IncludeAssets = element.AttributeOrElement("IncludeAssets");
                reference.ExcludeAssets = element.AttributeOrElement("ExcludeAssets");
                reference.PrivateAssets = element.AttributeOrElement("PrivateAsserts");
                if (reference.PrivateAssets != null && (reference.PrivateAssets.Contains("all") || reference.PrivateAssets.Contains("runtime")))
                    continue;
                if (reference.ExcludeAssets != null && (reference.ExcludeAssets.Contains("all") || reference.ExcludeAssets.Contains("runtime")))
                    continue;
                if (reference.IncludeAssets != null && !(reference.IncludeAssets.Contains("all") ||  reference.IncludeAssets.Contains("runtime")))
                    continue;
                refs.Add(reference);
            }
            return refs;
        }

        public class ProjectReference
        {
            public string ProjectPath { get; set; }

            public ProjectReference(string projectPath)
            {
                ProjectPath = projectPath;
            }
        }

        public IReadOnlyList<ProjectReference> FindProjectReferences()
        {
            List<ProjectReference> refs = new List<ProjectReference>();

            var elements = mRoot.Children("ItemGroup").Children("ProjectReference");
            foreach (var element in elements)
            {
                string path = element.Attribute("Include");
                if (!Path.IsPathRooted(path))
                    path = Path.GetFullPath(Path.Combine(Location, path));

                XmlElement parent = element.ParentNode as XmlElement;
                if (parent != null && parent.HasAttribute("Condition"))
                    throw new Exception("The condition of a property group that consists of a project reference is not supported");

                refs.Add(new ProjectReference(path));
            }
            return refs;
        }




    }
}

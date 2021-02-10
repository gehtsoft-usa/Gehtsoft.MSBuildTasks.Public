using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Gehtsoft.Build.Nuget
{
    public class NugetSpecificationFile
    {
        private readonly XmlDocument mDocument;
        private readonly XmlElement mRoot;
        private readonly XmlElement mMetadata;
        private readonly XmlElement mFiles;
        private readonly XmlElement mDependencies;
        private const string cPrefix = "nuget";
        private const string cUri = "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd";

        public NugetSpecificationFile()
        {
            mDocument = new XmlDocument();
            mRoot = mDocument.CreateRoot("package", cPrefix, cUri);
            mMetadata = mRoot.AddElement("metadata", cPrefix, cUri);
            mFiles = mRoot.AddElement("files", cPrefix, cUri);
            mDependencies = mMetadata.AddElement("dependencies", cPrefix, cUri);
        }

        public string Id
        {
            get => this["id"];
            set => this["id"] = value;
        }
        public string Version
        {
            get => this["version"];
            set => this["version"] = value;
        }
        public string Authors
        {
            get => this["authors"];
            set => this["authors"] = value;
        }
        public string Owners
        {
            get => this["owners"];
            set => this["owners"] = value;
        }
        public string LicenseUrl
        {
            get => this["licenseUrl"];
            set => this["licenseUrl"] = value;
        }

        public void SetLicense(string type, string licenseId)
        {
            XmlElement licenseElement = mMetadata.ChildOrCreate("license", cPrefix, cUri);
            licenseElement.AddAttribute("type", type);
            licenseElement.InnerText = licenseId;
        }

        public string ProjectUrl
        {
            get => this["projectUrl"];
            set => this["projectUrl"] = value;
        }
        public bool RequireLicenseAcceptance
        {
            get => this["requireLicenseAcceptance"] == "true";
            set => this["requireLicenseAcceptance"] = value ? "true" : "false";
        }
        public string Description
        {
            get => this["description"];
            set => this["description"] = value;
        }
        public string ReleaseNotes
        {
            get => this["releaseNotes"];
            set => this["releaseNotes"] = value;
        }

        public string this[string propName]
        {
            get => mMetadata.ChildOrCreate(propName, cPrefix, cUri).InnerText;
            set => mMetadata.ChildOrCreate(propName, cPrefix, cUri).InnerText = value;
        }


        public void AddDependency(string id, string version, string targetFramework)
        {
            XmlElement group;
            group = mDependencies.Children("group", cUri)
                .FirstOrDefault(e => e.Attribute("targetFramework") == targetFramework);
            if (group == null)
            {
                group = mDependencies.AddElement("group", cPrefix, cUri)
                    .AddAttribute("targetFramework", targetFramework);
            }

            XmlElement dep;
            dep = group.Children("dependency", cUri).FirstOrDefault(e => e.Attribute("id") == id);

            if (dep == null)
            {
                dep = group.AddElement("dependency", cPrefix, cUri)
                    .AddAttribute("id", id)
                    .AddAttribute("version", version);
            }
        }
        public void AddDependency(string id, string version)
        {
            XmlElement dep;
            dep = mDependencies.Children("dependency", cUri).FirstOrDefault(e => e.Attribute("id") == id);

            if (dep == null)
            {
                dep = mDependencies.AddElement("dependency", cPrefix, cUri)
                    .AddAttribute("id", id)
                    .AddAttribute("version", version);
            }
        }

        public void AddFile(string file, string target)
        {
            mFiles.AddElement("file", cPrefix, cUri)
                .AddAttribute("src", file)
                .AddAttribute("target", target);
        }

        public XmlDocument Document => mDocument;

        public void Save(string filename)
        {
            using (FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    NamespaceHandling = NamespaceHandling.OmitDuplicates
                };
                using (XmlWriter w = XmlWriter.Create(s, settings))
                {
                    mDocument.Save(w);
                }
            }

                
        }

        
    }
}
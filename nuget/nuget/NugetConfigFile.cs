using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Gehtsoft.Build.Nuget
{
    public class NugetConfigFile
    {
        private readonly XmlDocument mDocument;
        private readonly XmlElement mRoot;

        public class Version : IComparable<Version>
        {
            public string Id { get; }
            public string Description { get; }

            public Version(string id, string description)
            {
                Id = id;
                Description = description;
            }

            private int[] mIds = null;

            public int[] Ids
            {
                get
                {
                    if (mIds == null)
                    {
                        string[] ids = Id.Split('.');
                        int[] r = new int[ids.Length];
                        for (int i = 0; i < ids.Length; i++)
                        {
                            r[i] = int.Parse(ids[i]);
                        }

                        mIds = r;
                    }
                    return mIds;
                }
            }

            public int CompareTo(Version other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;

                int[] ids1 = Ids;
                int[] ids2 = other.Ids;
                int l = ids1.Length <= ids2.Length ? ids1.Length : ids2.Length;
                for (int i = 0; i < l; i++)
                {
                    if (ids1[i] > ids2[i])
                        return 1;
                    else if (ids1[i] < ids2[i])
                        return -1;
                }

                if (ids1.Length > ids2.Length)
                    return 1;
                else if (ids1.Length < ids2.Length)
                    return -1;
                return 0;
            }
        }

        public class VersionCollection : IReadOnlyList<Version>
        {
            private readonly List<Version> mList = new List<Version>();

            public Version Last
            {
                get
                {
                    if (mList.Count == 0)
                        return null;
                    else if (mList.Count == 1)
                        return mList[0];
                    else
                        return mList.Max();
                }
            }

            public int Count => mList.Count;

            public Version this[int index] => mList[index];
            public void Add(Version version) => mList.Add(version);
            public IEnumerator<Version> GetEnumerator() => mList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => mList.GetEnumerator();
        }

        private VersionCollection mVersions;

        public VersionCollection Versions
        {
            get
            {
                if (mVersions == null)
                {
                    mVersions = new VersionCollection();
                    XmlElement versions = mRoot.Children("versions", cUrl).FirstOrDefault();
                    foreach (XmlElement version in versions.Children("version", cUrl))
                    {
                        mVersions.Add(new Version(version.Attribute("id"), version.InnerText));
                    }
                }
                return mVersions;
            }
        }

        public class Project
        {
            public string Id { get; }
            public string SubVersion { get; }
            public bool HasSubVersion => !string.IsNullOrEmpty(SubVersion);
            public string Description { get; }
            public string Location { get; }
            public string TargetFolder { get; }

            public PropertyCollection Properties { get; } = new PropertyCollection();

            public class AdditionalFile
            {
                public string File { get; }
                public string Target { get; }

                public AdditionalFile(string file, string target)
                {
                    File = file;
                    Target = target;
                }
            }

            public class AdditionalFileCollection : IReadOnlyList<AdditionalFile>
            {
                private readonly List<AdditionalFile> mList = new List<AdditionalFile>();

                public int Count => mList.Count;

                public AdditionalFile this[int index] => mList[index];
                public void Add(AdditionalFile version) => mList.Add(version);
                public IEnumerator<AdditionalFile> GetEnumerator() => mList.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => mList.GetEnumerator();
            }

            public AdditionalFileCollection AdditionalFiles { get; } = new AdditionalFileCollection();

            public Project(string id, string subversion, string description, string location, string targetFolder)
            {
                Id = id;
                SubVersion = subversion;
                Description = description;
                Location = location;
                TargetFolder = targetFolder ?? "lib";
            }
        }

        public class ProjectCollection : IReadOnlyList<Project>
        {
            private readonly List<Project> mList = new List<Project>();

            public int Count => mList.Count;

            public Project this[int index] => mList[index];

            public Project this[string id]
            {
                get
                {
                    foreach (var project in mList)
                    {
                        if (project.Id == id)
                            return project;
                    }

                    return null;
                }
            }

            public void Add(Project project) => mList.Add(project);
            public IEnumerator<Project> GetEnumerator() => mList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => mList.GetEnumerator();
        }

        private ProjectCollection mProjects;
        private const string cUrl = "http://www.gehtsoft.com/build/nuget";

        public ProjectCollection Projects
        {
            get
            {
                if (mProjects == null)
                {
                    mProjects = new ProjectCollection();
                    XmlElement projects = mRoot.Children("projects", cUrl).FirstOrDefault();
                    foreach (XmlElement projectElement in projects.Children("project", cUrl))
                    {
                        var project = new Project(projectElement.Attribute("id"),
                            projectElement.Attribute("sub-version"),
                            projectElement.Children("description", cUrl).FirstOrDefault()?.InnerText,
                            projectElement.Attribute("location"), projectElement.Attribute("target-folder"));
                        mProjects.Add(project);
                        foreach (XmlElement customProperty in projectElement.Children("custom-property", cUrl))
                            project.Properties.Add(new Property(customProperty.Attribute("id"), customProperty.InnerText));
                        foreach (XmlElement additionalFile in projectElement.Children("additional-file", cUrl))
                            project.Properties.Add(new Property(additionalFile.Attribute("file"), additionalFile.Attribute("target")));
                    }
                }
                return mProjects;
            }
        }

        private List<string> mExcludeReference;

        public IReadOnlyList<string> ExcludeReference
        {
            get
            {
                if (mExcludeReference == null)
                {
                    mExcludeReference = new List<string>();
                    foreach (var exclusion in mRoot.Children("exclude-references", cUrl).FirstOrDefault().Children("exclude", cUrl))
                    {
                        var id = exclusion.Attribute("id");
                        if (!string.IsNullOrEmpty(id))
                            mExcludeReference.Add(id);
                    }
                }
                return mExcludeReference;
            }
        }

        public string Owner => mRoot.Children("properties", cUrl).FirstOrDefault()?.Children("owner", cUrl).FirstOrDefault()?.InnerText;
        public string ProjectUrl => mRoot.Children("properties", cUrl).FirstOrDefault()?.Children("projectUrl", cUrl).FirstOrDefault()?.InnerText;
        public string License => mRoot.Children("properties", cUrl).FirstOrDefault()?.Children("license", cUrl).FirstOrDefault()?.InnerText;
        public string LicenseUrl => mRoot.Children("properties", cUrl).FirstOrDefault()?.Children("licenseUrl", cUrl).FirstOrDefault()?.InnerText;
        public string Copyright => mRoot.Children("properties", cUrl).FirstOrDefault()?.Children("copyright", cUrl).FirstOrDefault()?.InnerText;

        public class Property 
        {
            public string Id { get; }
            public string Value { get; }

            public Property(string id, string value)
            {
                Id = id;
                Value = value;
            }
        }

        public class PropertyCollection : IReadOnlyList<Property>
        {
            private readonly List<Property> mList = new List<Property>();

            public int Count => mList.Count;

            public Property this[int index] => mList[index];
            public void Add(Property version) => mList.Add(version);
            public IEnumerator<Property> GetEnumerator() => mList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => mList.GetEnumerator();
        }

        private PropertyCollection mProperties;

        public PropertyCollection Properties
        {
            get
            {
                if (mProperties == null)
                {
                    mProperties = new PropertyCollection();
                    XmlElement versions = mRoot.Children("properties", cUrl).FirstOrDefault();
                    foreach (XmlElement property in versions.Children("custom-property", cUrl))
                    {
                        mProperties.Add(new Property(property.Attribute("id"), property.InnerText));
                    }
                }
                return mProperties;
            }
        }


        public NugetConfigFile(string file)
        {
            mDocument = new XmlDocument();
            mDocument.LoadFile(file, NugetSpecSchemaProvider.GetSchema());
            mRoot = mDocument.DocumentElement;
        }
        public NugetConfigFile(Stream file)
        {
            mDocument = new XmlDocument();
            mDocument.LoadFile(file, NugetSpecSchemaProvider.GetSchema());
            mRoot = mDocument.DocumentElement;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Gehtsoft.Build.Nuget
{
    public static class NugetSpecSchemaProvider
    {
        private static XmlSchema gSchema;

        public static XmlSchema GetSchema()
        {
            if (gSchema == null)
            {
                using (Stream stream = typeof(NugetSpecSchemaProvider).Assembly.GetManifestResourceStream($"Gehtsoft.Build.Nuget.nugetspec.xsd"))
                {
                    gSchema = XmlSchema.Read(stream, (sender, e) => throw e.Exception);
                }
            }

            return gSchema;
        }
    }
}

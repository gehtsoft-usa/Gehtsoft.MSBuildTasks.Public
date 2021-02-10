using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;

namespace Gehtsoft.Build.ContentDelivery
{
    public class GetContent : Microsoft.Build.Utilities.Task
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public bool Unzip { get; set; } = true;

        public override bool Execute()
        {
            string package;
            Log.LogMessage("Restoring {0} to {1}...", Source, Destination);
            using (var client = new WebClient())
            {
                package = Path.GetTempFileName();
                client.DownloadFile(Source, package);
            }

            FileInfo fi = new FileInfo(Destination);
            if (!Directory.Exists(fi.DirectoryName))
                Directory.CreateDirectory(fi.DirectoryName);

            if (Unzip)
            {
                using (var zip = ZipFile.Open(package, ZipArchiveMode.Read))
                {
                    if (!Directory.Exists(Destination))
                        Directory.CreateDirectory(Destination);
                    zip.ExtractToDirectory(Destination);
                }
            }
            else
                File.Copy(package, Destination, true);

            File.Delete(package);
            return true;
        }
    }
}

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
    public class ContentFromPackage : Microsoft.Build.Utilities.Task
    {
        public string Package { get; set; }
        public string Version { get; set; }
        public string Source { get; set; } = "Content";
        public string Destination { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Copying content of {0}/{1}/{2} to {3}...", Source, Version, Source, Destination);


            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string path = Path.Combine(profile, ".nuget", "packages");

            if (!Directory.Exists(path))
            {
                Log.LogError(".nuget cache folder isn't found in the system profile. Please check your configuration or whether nuget cache folder location is changed again with the new version...");
                return false;
            }

            path = Path.Combine(path, Package, Version);

            if (!Directory.Exists(path))
            {
                Log.LogError("The package {0} {1} does not exists. Please restore packages using nuget restore ({2})", Package, Version, path);
                return false;
            }

            path = Path.Combine(path, Source);
            if (!Directory.Exists(path))
            {
                Log.LogError("The source folder {2} is not found in the package {0} {1}", Package, Version, Source);
                return false;
            }


            if (Directory.Exists(Destination))
            {
                var di = Directory.CreateDirectory(Destination);
                if (di == null || !di.Exists)
                {
                    Log.LogError("The target directiory {0} does not exists and cannot be created", Destination);
                    return false;
                }
            }

            DirectoryCopy(path, Destination, true);

            return true;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}

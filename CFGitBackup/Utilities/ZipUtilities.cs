using System.IO.Compression;

namespace CFGitBackup.Utilities
{
    /// <summary>
    /// ZIP utilities
    /// </summary>
    public static class ZipUtilities
    {
        public static void CreateZipFromFolder(string folder, string zipFile)
        {
            using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                AddFolderToZip(archive, folder, "");

                /*
                foreach (var file in Directory.GetFiles(downloadFolder))
                {
                    archive.CreateEntryFromFile(file, Path.GetFileName(file));
                } 
                */

                archive.Dispose();
            }
        }
        
        private static void AddFolderToZip(ZipArchive archive, string folder, string entryName)
        {
            // Add files
            foreach (var file in Directory.GetFiles(folder))
            {
                var fileEntryName = String.IsNullOrEmpty(entryName) ? Path.GetFileName(file) : $"{entryName}/{Path.GetFileName(file)}";

                archive.CreateEntryFromFile(file, fileEntryName);
                //archive.CreateEntryFromFile(file, Path.GetFileName(file));
            }

            // Add sub-folders
            foreach (var subFolder in Directory.GetDirectories(folder))
            {
                var subFolderName = new DirectoryInfo(subFolder).Name;

                var subFolderEntryName = String.IsNullOrEmpty(entryName) ? $"{subFolderName}" : $"{entryName}/{subFolderName}";
                //archive.CreateEntry(subFolderEntryName);

                AddFolderToZip(archive, subFolder, subFolderEntryName);
            }
        }
    }
}

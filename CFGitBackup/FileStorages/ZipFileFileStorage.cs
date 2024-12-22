using CFGitBackup.Interfaces;
using System.IO.Compression;

namespace CFGitBackup.FileStorages
{
    /// <summary>
    /// File storage via .zip file
    /// </summary>
    public class ZipFileFileStorage : IFileStorage, IDisposable
    {
        private readonly string _zipFile;
        private ZipArchive? _archive;

        public ZipFileFileStorage(string zipFile)
        {
            _zipFile = zipFile;
        }

        public void Clear()
        {
            if (_archive != null)
            {
                _archive.Dispose();
                _archive = null;
            }

            if (File.Exists(_zipFile))
            {
                File.Delete(_zipFile);
            }
        }

        public void Dispose()
        {
            if (_archive != null)
            {
                _archive.Dispose();
                _archive = null;
            }            
        }

        public void Close()
        {
            Dispose();
        }

        private ZipArchive GetZipArchive()
        {
            if (_archive == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_zipFile));

                _archive = ZipFile.Open(_zipFile, ZipArchiveMode.Create);
            }
            return _archive;
        }

        private string GetArchiveEntryName(string[] folderNames, string fileName)
        {
            var entryName = "";
            if (folderNames.Length == 0)
            {
                entryName = fileName;
            }
            else     // Folder
            {
                foreach (var folderName in folderNames)
                {
                    if (entryName == "")
                    {
                        entryName = folderName;
                    }
                    else
                    {
                        entryName = $"{entryName}/{folderName}";
                    }
                }

                entryName = $"{entryName}/{fileName}";
            }

            return entryName;
        }
     
        public Task WriteFileAsync(string[] folderNames, string fileName, byte[] content)
        {         
            var tempFilePath = Path.GetTempFileName();

            try
            {
                // TODO: Find a way to write to archive without writing to file system first
                File.WriteAllBytes(tempFilePath, content);

                var entryName = GetArchiveEntryName(folderNames, fileName);
                
                var archive = GetZipArchive();                
                archive.CreateEntryFromFile(tempFilePath, entryName);                
            }
            finally
            {
                // Clean up
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return Task.CompletedTask;
        }
    }
}

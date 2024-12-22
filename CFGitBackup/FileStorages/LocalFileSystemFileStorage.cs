using CFGitBackup.Interfaces;

namespace CFGitBackup.FileStorages
{
    /// <summary>
    /// File storage via local file system folder. Class instance can only modify contents of root folder.
    /// </summary>
    public class LocalFileSystemFileStorage : IFileStorage
    {
        private readonly string _rootFolder;

        public LocalFileSystemFileStorage(string rootFolder)
        {
            _rootFolder = rootFolder;
        }

        public void Clear()
        {
            // Delete files
            foreach(var file in Directory.GetFiles(_rootFolder))
            {
                File.Delete(file);
            }

            // Delete sub-folders
            foreach(var subFolder in Directory.GetDirectories(_rootFolder))
            {
                Directory.Delete(subFolder, true);
            }
        }

        public void Close()
        {

        }        

        private string GetFolderPath(string[] folderNames)
        {
            var fileFolder = _rootFolder;
            foreach (var folderName in folderNames)
            {
                fileFolder = Path.Combine(fileFolder, folderName);
            }
            return fileFolder;
        }

        public async Task WriteFileAsync(string[] folderNames, string fileName, byte[] content)
        {
            var fileFolder = GetFolderPath(folderNames);            
            var filePath = Path.Combine(fileFolder, fileName);

            await File.WriteAllBytesAsync(filePath, content);
        }

        //public async Task DeleteFileAsync (string[] folderNames, string fileName)
        //{
        //    var fileFolder = GetFolderPath(folderNames);
        //    var filePath = Path.Combine(fileFolder, fileName);

        //    if (File.Exists(filePath))
        //    {
        //        File.Delete(filePath);
        //    }
        //}
    }
}

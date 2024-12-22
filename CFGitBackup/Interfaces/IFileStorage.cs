namespace CFGitBackup.Interfaces
{
    /// <summary>
    /// File storage interface. E.g. Local file system, .zip file.
    /// </summary>
    public interface IFileStorage
    {
        /// <summary>
        /// Clears all content
        /// </summary>
        void Clear();

        /// <summary>
        /// Writes file
        /// </summary>
        /// <param name="folderNames">Folder names (None=Root path)</param>
        /// <param name="fileName">File name</param>
        /// <param name="content">File content</param>
        /// <returns></returns>
        Task WriteFileAsync(string[] folderNames, string fileName, byte[] content);

        //Task DeleteFileAsync(string[] folderNames, string fileName);

        void Close();
    }
}

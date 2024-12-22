using CFGitBackup.Models;

namespace CFGitBackup.Interfaces
{
    /// <summary>
    /// Interface for accessing Git repo
    /// </summary>
    public interface IGitRepoService
    {
        string PlatformName { get; }

        void SetConfig(GitConfig gitConfig);

        /// <summary>
        /// Gets list of Git repos
        /// </summary>
        /// <returns></returns>
        Task<List<GitRepo>> GetAllReposAsync();
        
        /// <summary>
        /// Downloads Git repo to file storage (Local folder/zip file etc)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fileStorage"></param>
        /// <returns></returns>
        Task DownloadRepo(string name, IFileStorage fileStorage);
    }
}

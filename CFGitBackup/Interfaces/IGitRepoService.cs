using CFGitBackup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Downloads Git repo to local folder
        /// </summary>
        /// <param name="name"></param>
        /// <param name="localFolder"></param>
        /// <returns></returns>
        Task DownloadRepo(string name, string localFolder);
    }
}

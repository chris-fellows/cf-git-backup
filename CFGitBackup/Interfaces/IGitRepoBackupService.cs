using CFGitBackup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFGitBackup.Interfaces
{
    /// <summary>
    /// Service for backing up Git repos
    /// </summary>
    public interface IGitRepoBackupService
    {
        /// <summary>
        /// Backs up Git repo
        /// </summary>
        /// <param name="gitConfig"></param>        
        /// <param name="gitBackupConfig"></param>
        /// <returns></returns>
        Task BackupRepoAsync(GitConfig gitConfig, GitRepoBackupConfig gitBackupConfig, CancellationToken cancellationToken);

        /// <summary>
        /// Get overdue backups
        /// </summary>
        /// <returns></returns>
        List<GitRepoBackupConfig> GetOverdueBackups();
    }
}

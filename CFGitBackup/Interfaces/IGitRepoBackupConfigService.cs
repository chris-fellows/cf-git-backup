using CFGitBackup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFGitBackup.Interfaces
{
    /// <summary>
    /// Service for managing GitRepoBackupConfig instances
    /// </summary>
    public interface IGitRepoBackupConfigService : IEntityWithIdStoreService<GitRepoBackupConfig, string>
    {
    }
}

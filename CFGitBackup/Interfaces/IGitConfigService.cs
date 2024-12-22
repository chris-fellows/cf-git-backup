using CFGitBackup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFGitBackup.Interfaces
{
    /// <summary>
    /// Service for managing GitConfig instances
    /// </summary>
    public interface IGitConfigService : IEntityWithIdStoreService<GitConfig, string>
    {

    }
}

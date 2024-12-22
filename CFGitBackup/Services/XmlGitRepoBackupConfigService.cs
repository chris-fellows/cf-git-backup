using CFGitBackup.Interfaces;
using CFGitBackup.Models;

namespace CFGitBackup.Services
{
    /// <summary>
    /// XML storage of GitRepoBackupConfig instances
    /// </summary>
    public class XmlGitRepoBackupConfigService : XmlEntityWithIdStoreService<GitRepoBackupConfig, string>, IGitRepoBackupConfigService
    {
        public XmlGitRepoBackupConfigService(string folder) : base(folder,
                                            "GitRepoBackupConfig.*.xml",
                                            (gitRepoBackupConfig) => $"GitRepoBackupConfig.{gitRepoBackupConfig.Id}.xml",
                                            (gitRepoBackupConfigId) => $"GitRepoBackupConfig.{gitRepoBackupConfigId}.xml")
        {

        }
    }
}

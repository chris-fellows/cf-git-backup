using CFGitBackup.Interfaces;
using CFGitBackup.Models;

namespace CFGitBackup.Services
{
    /// <summary>
    /// XML storage of GitConfig instances
    /// </summary>
    public class XmlGitConfigService : XmlEntityWithIdStoreService<GitConfig, string>, IGitConfigService
    {
        public XmlGitConfigService(string folder) : base(folder,
                                            "GitConfig.*.xml",
                                            (gitConfig) => $"GitConfig.{gitConfig.Id}.xml",
                                            (gitConfigId) => $"GitConfig.{gitConfigId}.xml")
        {

        }
    }
}

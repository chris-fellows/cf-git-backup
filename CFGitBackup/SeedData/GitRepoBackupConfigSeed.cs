using CFGitBackup.Constants;
using CFGitBackup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFGitBackup.SeedData
{
    /// <summary>
    /// Seed for GitRepoBackupConfig instances
    /// </summary>
    public class GitRepoBackupConfigSeed
    {
        public List<GitRepoBackupConfig> GetAll(List<GitRepo> gitRepos, string localFolder)
        {
            var configs = new List<GitRepoBackupConfig>();

            foreach (var repo in gitRepos)
            {
                var config = new GitRepoBackupConfig()
                {
                    Id = Guid.NewGuid().ToString(),
                    GitConfigId = PlatformNames.GitHub,
                    Compressed = true,
                    CompressedFileName = $"{repo.Name}.zip",
                    LocalFolder = localFolder,
                    RepoName = repo.Name,
                    Enabled = true,
                    BackupFrequency = TimeSpan.FromHours(24),
                    LastBackUpDate = DateTime.MinValue
                };
                configs.Add(config);
            }

            return configs;
        }
    }
}

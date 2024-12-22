using CFGitBackup.Constants;
using CFGitBackup.Models;

namespace CFGitBackup.SeedData
{
    /// <summary>
    /// Seed for GitConfig instances
    /// </summary>
    public class GitConfigSeed
    {
        public List<GitConfig> GetAll()
        {
            var gitConfigs = new List<GitConfig>();

            var gitConfig1 = new GitConfig()
            {
                Id = PlatformNames.GitHub,
                PlatformName = PlatformNames.GitHub,
                APIBaseURL = "https://api.github.com",
                Owner = "chris-fellows",
                Token = File.ReadAllText("D:\\Data\\Dev\\C#\\cf-git-backup-local\\GitHubAccessToken.txt") 
            };
            gitConfigs.Add(gitConfig1);

            return gitConfigs;
        }
    }
}

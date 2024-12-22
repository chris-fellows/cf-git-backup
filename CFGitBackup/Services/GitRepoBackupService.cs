using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using CFGitBackup.Utilities;

namespace CFGitBackup.Services
{
    public class GitRepoBackupService : IGitRepoBackupService
    {
        private readonly List<IGitRepoService> _gitRepoServices;
        private readonly IGitRepoBackupConfigService _gitRepoBackupConfigService;

        public GitRepoBackupService(IGitRepoBackupConfigService gitRepoBackupConfigService,
                                    IEnumerable<IGitRepoService> gitRepoServices)
        {
            _gitRepoBackupConfigService = gitRepoBackupConfigService;
            _gitRepoServices = gitRepoServices.ToList();
        }

        public Task BackupRepoAsync(GitConfig gitConfig, GitRepoBackupConfig gitBackupConfig)
        {
            var task = Task.Factory.StartNew(() =>
            {
                // Get Git repo service
                var gitRepoService = _gitRepoServices.First(s => s.PlatformName == gitConfig.PlatformName);
                gitRepoService.SetConfig(gitConfig);

                // Set temp folder for downloading Git repo to, only used if we need to create .zip
                var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                
                // Set folder to download Git repo to. If compressed then download to temp folder else directly to local folder
                var downloadFolder = gitBackupConfig.Compressed ? tempFolder : gitBackupConfig.LocalFolder;

                Directory.CreateDirectory(gitBackupConfig.LocalFolder);
                Directory.CreateDirectory(downloadFolder);

                // Download Git repo
                gitRepoService.DownloadRepo(gitBackupConfig.RepoName, downloadFolder).Wait();

                // Compress if requires
                if (gitBackupConfig.Compressed)
                {
                    // Create .zip
                    // TODO: Support placeholders in CompressedFileName (E.g. #CURRENT_DATE#)
                    var zipFile = Path.Combine(gitBackupConfig.LocalFolder,
                                    string.IsNullOrEmpty(gitBackupConfig.CompressedFileName) ? $"{gitBackupConfig.RepoName}.zip" : gitBackupConfig.CompressedFileName);

                    ZipUtilities.CreateZipFromFolder(downloadFolder, zipFile);

                    Directory.Delete(tempFolder, true);
                }

                // Update last backup
                var gitBackupConfigCurrent = _gitRepoBackupConfigService.GetById(gitBackupConfig.Id);
                gitBackupConfigCurrent.LastBackUpDate = DateTimeOffset.UtcNow;
                _gitRepoBackupConfigService.Update(gitBackupConfigCurrent);
            });

            return task;
        }

        public List<GitRepoBackupConfig> GetOverdueBackups()
        {                                
            var now = DateTimeOffset.UtcNow;

            var gitRepoBackupConfigs = _gitRepoBackupConfigService.GetAll();

            var overdue = gitRepoBackupConfigs.Where(c => c.Enabled &&
                    c.LastBackUpDate.Add(c.BackupFrequency) <= now)
                .OrderBy(c => c.LastBackUpDate).ToList();
            
            return overdue;
        }
    }
}

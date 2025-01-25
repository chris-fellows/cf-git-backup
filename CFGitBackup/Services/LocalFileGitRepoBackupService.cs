using CFGitBackup.FileStorages;
using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using CFGitBackup.Utilities;
using System.IO.Compression;

namespace CFGitBackup.Services
{
    /// <summary>
    /// Backups up Git repo to local file system
    /// </summary>
    public class LocalFileGitRepoBackupService : IGitRepoBackupService
    {
        private readonly List<IGitRepoService> _gitRepoServices;
        private readonly IGitRepoBackupConfigService _gitRepoBackupConfigService;

        public LocalFileGitRepoBackupService(IGitRepoBackupConfigService gitRepoBackupConfigService,
                                    IEnumerable<IGitRepoService> gitRepoServices)
        {
            _gitRepoBackupConfigService = gitRepoBackupConfigService;
            _gitRepoServices = gitRepoServices.ToList();
        }

        public Task BackupRepoAsync(GitConfig gitConfig, GitRepoBackupConfig gitBackupConfig,
                                    CancellationToken cancellationToken)
        {
            // Check parameters
            if (String.IsNullOrEmpty(gitBackupConfig.LocalFolder))
            {
                throw new ArgumentException("Local folder must be set");
            }
            if (String.IsNullOrEmpty(gitBackupConfig.RepoName))
            {
                throw new ArgumentException("Repo name must be set");
            }

            var task = Task.Factory.StartNew(() =>
            {              
                // Get Git repo service
                var gitRepoService = _gitRepoServices.First(s => s.PlatformName == gitConfig.PlatformName);
                gitRepoService.SetConfig(gitConfig);
                
                Directory.CreateDirectory(gitBackupConfig.LocalFolder);

                // Set zip file to use
                var zipFile = Path.Combine(gitBackupConfig.LocalFolder,
                                string.IsNullOrEmpty(gitBackupConfig.CompressedFileName) ? $"{gitBackupConfig.RepoName}.zip" : gitBackupConfig.CompressedFileName);

                // Set IFileStorage
                // TODO: Extend this to include other cloud storage formats
                IFileStorage fileStorage = gitBackupConfig.Compressed switch
                {
                    true => new ZipFileFileStorage(zipFile),
                    _ => new LocalFileSystemFileStorage(gitBackupConfig.LocalFolder)
                };

                // Clear existing content
                fileStorage.Clear();

                // Download
                var isDownloadSuccess = false;
                try
                {
                    // Download Git repo
                    gitRepoService.DownloadRepo(gitBackupConfig.RepoName, fileStorage, cancellationToken).Wait();
                    isDownloadSuccess = true;
                }                
                finally
                {

                    // Flush changes
                    fileStorage.Close();

                    // If failed then clean up partial backup
                    if (!isDownloadSuccess)
                    {
                        fileStorage.Clear();
                    }
                }

                /*
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
                */

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

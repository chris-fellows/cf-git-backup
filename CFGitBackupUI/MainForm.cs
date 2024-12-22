using CFGitBackup;
using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using CFGitBackup.SeedData;
using CFGitBackup.Services;
using System.Configuration;
using System.Security.Permissions;

namespace CFGitBackupUI
{
    public partial class MainForm : Form
    {
        private System.Timers.Timer _timer = null;
        private Task? _backupRepoTask;
        private GitRepoBackupConfig? _backupGitRepoBackupConfig;

        private readonly IGitConfigService _gitConfigService;
        private readonly IGitRepoBackupConfigService _gitRepoBackupConfigService;
        private readonly IGitRepoBackupService _gitRepoBackupService;
        private readonly List<IGitRepoService> _gitRepoServices;

        public MainForm(IGitConfigService gitConfigService,
                        IGitRepoBackupConfigService gitRepoBackupConfigService,
                        IGitRepoBackupService gitRepoBackupService,
                        IEnumerable<IGitRepoService> gitRepoServices)
        {
            InitializeComponent();

            _gitConfigService = gitConfigService;
            _gitRepoBackupConfigService = gitRepoBackupConfigService;
            _gitRepoBackupService = gitRepoBackupService;
            _gitRepoServices = gitRepoServices.ToList();

            DisplayStatus("Initialising");

            // Initialize sed data if required
            InitialiseSeedDataAsync().Wait();

            DisplayGitRepoBackupConfigs();

            // Check run mode
            if (Environment.GetCommandLineArgs().Contains("/Tray"))    // Run continuously in system tray
            {
                RunInTray();
            }
            else if (Environment.GetCommandLineArgs().Contains("/Silent"))    // Run and shut down (Called from Windows scheduler)
            {
                RunSilent();
            }
            else
            {
                RunInteractive();
            }

            //TestBackupRepoAsync().Wait();

            DisplayStatus("Ready");

            int xxx = 1000;
        }

        /// <summary>
        /// Initialises seed data if required
        /// </summary>
        /// <returns></returns>
        private async Task InitialiseSeedDataAsync()
        {
            var gitConfigs = _gitConfigService.GetAll();
            if (!gitConfigs.Any())
            {
                var defaultBackupFolder = System.Configuration.ConfigurationManager.AppSettings.Get("DefaultBackupFolder");

                gitConfigs = new GitConfigSeed().GetAll();
                gitConfigs.ForEach(gitConfig => _gitConfigService.Update(gitConfig));

                var gitRepos = new List<GitRepo>();

                foreach (var gitRepoService in _gitRepoServices)
                {
                    // Set Git config
                    var gitConfig = gitConfigs.First(c => c.PlatformName == gitRepoService.PlatformName);
                    gitRepoService.SetConfig(gitConfig);

                    // Get Git repos
                    var gitReposCurrent = await gitRepoService.GetAllReposAsync();

                    gitRepos.AddRange(gitReposCurrent);
                }

                var backupConfigs = new GitRepoBackupConfigSeed().GetAll(gitRepos, defaultBackupFolder);
                backupConfigs.ForEach(backupConfig => _gitRepoBackupConfigService.Update(backupConfig));
            }
        }

        //private void TestDownloadRepo()
        //{
        //    var gitConfig = new GitConfig();
        //    IGitRepoService gitRepoService = new GitHubGitRepoService();
        //    gitRepoService.SetConfig(gitConfig);

        //    gitRepoService.DownloadRepo("cf-sync-music", "D:\\Test\\CFGitBackup\\cf-sync-music").Wait();

        //    var repos = gitRepoService.GetAllReposAsync().Result;
        //}

        //private async Task TestBackupRepoAsync()
        //{
        //    var gitConfig = new GitConfig();
        //    IGitRepoService gitRepoService = new GitHubGitRepoService();
        //    gitRepoService.SetConfig(gitConfig);

        //    var repos = gitRepoService.GetAllReposAsync().Result;

        //    var repo = repos.First(r => r.Name == "cf-document-indexer");//

        //    var backupManager = new GitRepoBackupService(new List<IGitRepoService>() { gitRepoService });

        //    var backupConfig = new GitRepoBackupConfig()
        //    {
        //        Compressed = true,
        //        LocalFolder = "D:\\Test\\CFGitBackup\\Backups"
        //    };

        //    await backupManager.BackupRepoAsync(gitConfig, backupConfig);
        //}

        private void RunSilent()
        {

        }

        private void RunInTray()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 10000 * 1;    // Run soon after launch
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _timer.Enabled = false;
            }
            catch (Exception exception)
            {
                CheckRepoTaskResult();

                // Run overdue backups
                if (_backupRepoTask == null)
                {
                    var gitRepoBackupConfig = _gitRepoBackupService.GetOverdueBackups().FirstOrDefault();

                    if (gitRepoBackupConfig != null)    // Overdue
                    {
                        // Get Git config
                        var gitConfig = _gitConfigService.GetById(gitRepoBackupConfig.GitConfigId);

                        // Start backup                        
                        DisplayStatus($"Backing up {gitRepoBackupConfig.RepoName}");
                        SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, "Backing up");

                        _backupGitRepoBackupConfig = gitRepoBackupConfig;
                        _backupRepoTask = _gitRepoBackupService.BackupRepoAsync(gitConfig, gitRepoBackupConfig);
                    }
                }

                CheckRepoTaskResult();
            }
            finally
            {
                _timer.Interval = _backupRepoTask == null ? 60000 : 10000;
                _timer.Enabled = true;
            }
        }

        private void RunInteractive()
        {

        }

        private void CheckRepoTaskResult()
        {
            if (_backupRepoTask != null &&
                _backupRepoTask.IsCompleted)
            {
                if (_backupRepoTask.Exception != null)
                {
                    SetGitRepoBackupConfigStatus(_backupGitRepoBackupConfig.Id, $"Error: {_backupRepoTask.Exception.Message}");
                }
                else
                {
                    SetGitRepoBackupConfigStatus(_backupGitRepoBackupConfig.Id, "Backed up");
                }

                _backupRepoTask = null;
                _backupGitRepoBackupConfig = null;
            }
        }

        private void DisplayStatus(string status)
        {
            tsslStatus.Text = $" {status}";
        }

        private void DisplayGitRepoBackupConfigs()
        {
            var gitConfigs = _gitConfigService.GetAll();
            var gitRepoBackupConfigs = _gitRepoBackupConfigService.GetAll();

            dgvGitRepoBackupConfig.Rows.Clear();
            dgvGitRepoBackupConfig.Columns.Clear();

            int columnIndex = dgvGitRepoBackupConfig.Columns.Add("Repo", "Repo");
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Platform", "Platform");
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Last Backup", "Last Backup");
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Folder", "Folder");
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Status", "Status");

            foreach (var gitRepoBackupConfig in gitRepoBackupConfigs)
            {
                var gitConfig = gitConfigs.First(c => c.Id == gitRepoBackupConfig.GitConfigId);

                using (var row = new DataGridViewRow())
                {
                    row.Tag = gitRepoBackupConfig;

                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = gitRepoBackupConfig.RepoName;
                        row.Cells.Add(cell);
                    }
                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = gitConfig.PlatformName;
                        row.Cells.Add(cell);
                    }
                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = gitRepoBackupConfig.LastBackUpDate == DateTimeOffset.MinValue ? "None" :
                                            gitRepoBackupConfig.LastBackUpDate.ToString();
                        row.Cells.Add(cell);
                    }
                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = gitRepoBackupConfig.LocalFolder;
                        row.Cells.Add(cell);
                    }
                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = "";
                        row.Cells.Add(cell);
                    }

                    dgvGitRepoBackupConfig.Rows.Add(row);
                }
            }
        }

        private void SetGitRepoBackupConfigStatus(string gitRepoBackupConfigId, string status)
        {
            for (int rowIndex = 0; rowIndex < dgvGitRepoBackupConfig.Rows.Count; rowIndex++)
            {
                var config = (GitRepoBackupConfig)dgvGitRepoBackupConfig.Rows[rowIndex].Tag;
                if (config.Id == gitRepoBackupConfigId)
                {
                    var cell = (DataGridViewTextBoxCell)dgvGitRepoBackupConfig.Rows[rowIndex].Cells["Status"];
                    cell.Value = status;
                    break;
                }
            }
        }

        private void backUpNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Back up all Git repos?", "Back Up", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DisplayStatus("Backing up Git repos");

                var gitRepoBackupConfigs = _gitRepoBackupConfigService.GetAll().Where(c => c.Enabled).ToList();

                foreach (var gitRepoBackupConfig in gitRepoBackupConfigs)
                {
                    var gitConfig = _gitConfigService.GetById(gitRepoBackupConfig.GitConfigId);

                    DisplayStatus($"Backing up {gitRepoBackupConfig.RepoName}");                    
                    SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, "Backing up");

                    _backupGitRepoBackupConfig = gitRepoBackupConfig;
                    _backupRepoTask = _gitRepoBackupService.BackupRepoAsync(gitConfig, gitRepoBackupConfig);

                    // Wait for completion
                    while(!_backupRepoTask.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }

                    DisplayStatus($"Backed up {gitRepoBackupConfig.RepoName}");
                    SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, "Backed up");

                    _backupGitRepoBackupConfig = null;
                    _backupRepoTask = null;
                }

                DisplayStatus("Ready");
            }
        }
    }
}

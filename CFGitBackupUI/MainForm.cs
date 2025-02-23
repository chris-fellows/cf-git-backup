using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using CFGitBackup.SeedData;
using Microsoft.Extensions.DependencyInjection;

namespace CFGitBackupUI
{
    public partial class MainForm : Form
    {
        private enum RunModes : byte
        {
            Interactive,
            Silent,
            Tray
        }

        /// <summary>
        /// Details of active backup
        /// </summary>
        private class ActiveBackup
        {
            public IServiceScope? ServiceScope { get; set; }

            public Task? BackupRepoTask { get; set; }

            public GitRepoBackupConfig? BackupConfig { get; set; }

            public IGitRepoBackupService? BackupService { get; set; }
        }

        private const int _maxActiveBackups = 3;        // TODO: Move to config

        private System.Timers.Timer? _timer = null;        
        private RunModes _runMode;

        private List<ActiveBackup> _activeBackups = new List<ActiveBackup>();

        private CancellationTokenSource? _cancellationTokenSource;

        private readonly IGitConfigService _gitConfigService;
        private readonly IGitRepoBackupConfigService _gitRepoBackupConfigService;
        private readonly IGitRepoBackupService _gitRepoBackupService;   // Only needed to get overdue backups
        private readonly List<IGitRepoService> _gitRepoServices;
        private readonly IServiceProvider _serviceProvider;

        private readonly List<string> _backupConfigIdQueue = new List<string>();

        public MainForm(IGitConfigService gitConfigService,
                        IGitRepoBackupConfigService gitRepoBackupConfigService,
                        IGitRepoBackupService gitRepoBackupService,
                        IEnumerable<IGitRepoService> gitRepoServices,
                        IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _gitConfigService = gitConfigService;
            _gitRepoBackupConfigService = gitRepoBackupConfigService;
            _gitRepoBackupService = gitRepoBackupService;
            _gitRepoServices = gitRepoServices.ToList();
            _serviceProvider = serviceProvider;

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

            DisplayStatus("Ready");
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

        private void RunSilent()
        {
            _runMode = RunModes.Silent;
           
            WindowState = FormWindowState.Minimized;
        }

        private void RunInTray()
        {
            _runMode = RunModes.Tray;
            niNotify.Icon = this.Icon;      // SystemIcons.Application doesn't work
            niNotify.Text = "Git Backup - Idle";

            backUpNowToolStripMenuItem.Enabled = false;            

            // Enable timer
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 10000 * 1;    // Run soon after launch
            _timer.Enabled = true;

            WindowState = FormWindowState.Minimized;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            int shortInterval = 10000;  // Default
            try
            {
                _timer.Enabled = false;

                CheckRepoTaskResults();

                switch (_runMode)
                {
                    case RunModes.Interactive:
                        // Run next queued backup if any
                        shortInterval = 1000;   // So that we start next backup ASAP if required
                        if (_activeBackups.Count < _maxActiveBackups &&
                            _backupConfigIdQueue.Any())
                        {
                            // Get config
                            var gitRepoBackupConfig = _gitRepoBackupConfigService.GetById(_backupConfigIdQueue.First());

                            // Start backup (Removes from queue)
                            var activeBackup = StartRepoTask(gitRepoBackupConfig);
                            _activeBackups.Add(activeBackup);
                        }
 
                        break;
                    case RunModes.Tray:
                        // Run overdue backups
                        shortInterval = 5000;
                        if (_activeBackups.Count < _maxActiveBackups)
                        {
                            // Get overdue backups
                            var gitRepoBackupConfigs = _gitRepoBackupService.GetOverdueBackups();

                            // Exclude active backups
                            gitRepoBackupConfigs.RemoveAll(c => _activeBackups.Select(b => b.BackupConfig.Id).Contains(c.Id));

                            if (gitRepoBackupConfigs.Any())                            
                            {
                                var activeBackup = StartRepoTask(gitRepoBackupConfigs.First());
                                _activeBackups.Add(activeBackup);

                                if (gitRepoBackupConfigs.Count > 1) shortInterval = 100;
                            }
                        }

                        break;
                }

                CheckRepoTaskResults();
            }
            finally
            {                
                _timer.Interval = !_activeBackups.Any() ? 60000 : shortInterval;
                _timer.Enabled = true;
            }
        }

        /// <summary>
        /// Starts task to backup Git repo
        /// </summary>
        /// <param name="gitRepoBackupConfig"></param>
        private ActiveBackup StartRepoTask(GitRepoBackupConfig gitRepoBackupConfig)
        {            
            // Create cancellation token
            _cancellationTokenSource = new CancellationTokenSource();

            // Remove from queue if exists
            _backupConfigIdQueue.RemoveAll(c => c == gitRepoBackupConfig.Id);

            // Get Git config
            var gitConfig = _gitConfigService.GetById(gitRepoBackupConfig.GitConfigId);
                    
            // Update UI status in UI thread
            this.Invoke((Action)delegate
            {
                try
                {
                    DisplayStatus($"Backing up repos");
                    SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, "Backing up");
                }
                catch { };  // Ignore
            });

            var activeBackup = new ActiveBackup()
            {
                ServiceScope = _serviceProvider.CreateScope(),
                BackupConfig = gitRepoBackupConfig
            };

            activeBackup.BackupService = activeBackup.ServiceScope.ServiceProvider.GetRequiredService<IGitRepoBackupService>();

            activeBackup.BackupRepoTask = activeBackup.BackupService.BackupRepoAsync(gitConfig, gitRepoBackupConfig, _cancellationTokenSource.Token);
            
            return activeBackup;
        }

        private void RunInteractive()
        {
            _runMode = RunModes.Interactive;

            backUpNowToolStripMenuItem.Enabled = true;

            // Initialise timer
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = false;
            _timer.Interval = 60000;

            WindowState = FormWindowState.Normal;
        }

        private void CheckRepoTaskResults()
        {
            var completed = new List<ActiveBackup>();
            foreach(var activeBackup in _activeBackups)
            {
                if (CheckRepoTaskResult(activeBackup))
                {
                    completed.Add(activeBackup);
                }
            }

            _activeBackups.RemoveAll(b => completed.Contains(b));

            if (!_activeBackups.Any())
            {
                // If no more queued backups then re-enable Backup All menu item
                if (!_backupConfigIdQueue.Any() &&
                    _runMode == RunModes.Interactive)
                {
                    backUpNowToolStripMenuItem.Visible = true;
                    cancelBackupsToolStripMenuItem.Visible = false;
                }

                this.Invoke((Action)delegate
                {
                    DisplayStatus("Ready");
                });
            }
        }

        /// <summary>
        /// Checks status of backup
        /// </summary>
        /// <param name="activeBackup"></param>
        /// <returns>true: Completed; false: Still active</returns>
        private bool CheckRepoTaskResult(ActiveBackup activeBackup)
        {
            if (activeBackup.BackupRepoTask.IsCompleted)
            {
                this.Invoke((Action)delegate
                {
                    var backupConfig = _gitRepoBackupConfigService.GetById(activeBackup.BackupConfig.Id);

                    try
                    {
                        // Set backup status
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            SetGitRepoBackupConfigStatus(activeBackup.BackupConfig.Id, $"Cancelled");
                        }
                        else if (activeBackup.BackupRepoTask.Exception != null)
                        {
                            SetGitRepoBackupConfigStatus(activeBackup.BackupConfig.Id, $"Error: {activeBackup.BackupRepoTask.Exception.Message}");
                        }
                        else
                        {
                            SetGitRepoBackupConfigStatus(activeBackup.BackupConfig.Id, "Backed up");
                        }

                        // Set last backup date
                        SetGitRepoBackupConfigLastBackupDate(activeBackup.BackupConfig.Id, backupConfig.LastBackUpDate);
                    }
                    catch { };  // Ignore
                });           
            }

            return activeBackup.BackupRepoTask.IsCompleted;      
        }

        private void DisplayStatus(string status)
        {
            tsslStatus.Text = $" {status}";
            statusStrip1.Update();
        }

        private void DisplayGitRepoBackupConfigs()
        {
            var gitConfigs = _gitConfigService.GetAll();
            var gitRepoBackupConfigs = _gitRepoBackupConfigService.GetAll();

            dgvGitRepoBackupConfig.Rows.Clear();
            dgvGitRepoBackupConfig.Columns.Clear();

            int columnIndex = dgvGitRepoBackupConfig.Columns.Add("Repo", "Repo");
            dgvGitRepoBackupConfig.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Platform", "Platform");
            dgvGitRepoBackupConfig.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Last Backup", "Last Backup");
            dgvGitRepoBackupConfig.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Folder", "Folder");
            dgvGitRepoBackupConfig.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvGitRepoBackupConfig.Columns.Add("Status", "Status");
            dgvGitRepoBackupConfig.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            foreach (var gitRepoBackupConfig in gitRepoBackupConfigs.OrderBy(c => c.RepoName))
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

            dgvGitRepoBackupConfig.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
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

        private void SetGitRepoBackupConfigLastBackupDate(string gitRepoBackupConfigId, DateTimeOffset lastBackupDate)
        {
            for (int rowIndex = 0; rowIndex < dgvGitRepoBackupConfig.Rows.Count; rowIndex++)
            {
                var config = (GitRepoBackupConfig)dgvGitRepoBackupConfig.Rows[rowIndex].Tag;
                if (config.Id == gitRepoBackupConfigId)
                {
                    var cell = (DataGridViewTextBoxCell)dgvGitRepoBackupConfig.Rows[rowIndex].Cells["Last Backup"];
                    cell.Value = lastBackupDate == DateTimeOffset.MinValue ? "None" : lastBackupDate.ToString();
                    break;
                }
            }
        }

        private void backUpNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Back up all Git repos?", "Back Up", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //DisplayStatus("Backing up Git repos");

                var gitRepoBackupConfigs = _gitRepoBackupConfigService.GetAll().Where(c => c.Enabled).ToList();

                // Add backup configs to queue
                _backupConfigIdQueue.Clear();
                _backupConfigIdQueue.AddRange(gitRepoBackupConfigs.Select(c => c.Id));

                // Disable until all backups complete
                backUpNowToolStripMenuItem.Visible = false;
                cancelBackupsToolStripMenuItem.Visible = true;

                // Enable timer to process queue
                _timer.Interval = 500;                
                _timer.Enabled = true;

                //foreach (var gitRepoBackupConfig in gitRepoBackupConfigs)
                //{
                //    var gitConfig = _gitConfigService.GetById(gitRepoBackupConfig.GitConfigId);

                //    DisplayStatus($"Backing up {gitRepoBackupConfig.RepoName}");
                //    SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, "Backing up");

                //    // Start backup task
                //    _backupGitRepoBackupConfig = gitRepoBackupConfig;
                //    _backupRepoTask = _gitRepoBackupService.BackupRepoAsync(gitConfig, gitRepoBackupConfig);

                //    // Wait for completion
                //    while (!_backupRepoTask.IsCompleted)
                //    {
                //        System.Threading.Thread.Sleep(500);
                //    }

                //    // Display status
                //    if (_backupRepoTask.IsFaulted)
                //    {
                //        DisplayStatus($"Error backing up {gitRepoBackupConfig.RepoName}");
                //        SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, $"Error: {_backupRepoTask.Exception.Message}");
                //    }
                //    else
                //    {
                //        DisplayStatus($"Backed up {gitRepoBackupConfig.RepoName}");
                //        SetGitRepoBackupConfigStatus(gitRepoBackupConfig.Id, "Backed up");
                //    }

                //    _backupGitRepoBackupConfig = null;
                //    _backupRepoTask = null;
                //}

                //DisplayStatus("Ready");
            }
        }

        private void niNotify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_runMode== RunModes.Tray)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If closing form with task tray then prompt user
            if (e.CloseReason == CloseReason.UserClosing && _runMode == RunModes.Tray)
            {
                if (MessageBox.Show("Close application?", "Close", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            niNotify.Dispose();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_runMode == RunModes.Tray && FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void debugListAllReposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get Git config
            var gitConfig = _gitConfigService.GetAll().First();

            // Get Git repo service
            var gitRepoService = _gitRepoServices.First();
            gitRepoService.SetConfig(gitConfig);

            var gitRepos = gitRepoService.GetAllReposAsync().Result;

            int xxx = 1000;
        }

        private void cancelBackupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Cancel backups?", "Cancel Backup", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _backupConfigIdQueue.Clear();
                _cancellationTokenSource.Cancel();
            }
        }
    }
}

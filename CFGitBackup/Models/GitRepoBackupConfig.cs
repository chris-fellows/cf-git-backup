using System.Diagnostics.Contracts;

namespace CFGitBackup.Models
{
    /// <summary>
    /// Git repo backup config
    /// </summary>
    public class GitRepoBackupConfig
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Git config
        /// </summary>
        public string GitConfigId { get; set; } = String.Empty;

        /// <summary>
        /// Repo
        /// </summary>
        public string RepoName { get; set; } = String.Empty;

        /// <summary>
        /// Local folder to back up to
        /// </summary>
        public string LocalFolder { get; set; } = String.Empty;

        /// <summary>
        /// Whether to compress
        /// </summary>
        public bool Compressed { get; set; } = true;

        /// <summary>
        /// Compressed file name. May contain placeholders that are replaced at runtime (E.g. "cf-my-repo-#CURRENT_DATE")
        /// </summary>
        public string CompressedFileName { get; set; } = String.Empty;

        /// <summary>
        /// Whether backup is enabled
        /// </summary>
        public bool Enabled { get; set; }

        public TimeSpan BackupFrequency { get; set; } = TimeSpan.FromDays(1);

        public DateTimeOffset LastBackUpDate { get; set; } = DateTimeOffset.MinValue;
    }
}

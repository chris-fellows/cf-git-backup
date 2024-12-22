namespace CFGitBackup.Models
{
    /// <summary>
    /// Config for Git platform
    /// </summary>
    public class GitConfig
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Platform name
        /// </summary>
        public string PlatformName { get; set; } = String.Empty;

        /// <summary>
        /// Repo owner
        /// </summary>
        public string Owner { get; set; } = String.Empty;

        /// <summary>
        /// Access token for repo
        /// </summary>
        public string Token { get; set; } = String.Empty;

        /// <summary>
        /// Base URL for platform
        /// </summary>
        public string APIBaseURL { get; set; } = String.Empty;
    }
}

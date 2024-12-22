using CFGitBackup.Constants;
using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CFGitBackup.Services
{
    /// <summary>
    /// Git repo service for GitHub
    /// </summary>
    public class GitHubGitRepoService : IGitRepoService
    {
        private GitConfig? _config;

        private const string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36 Edg/96.0.1054.29";

        public GitHubGitRepoService()
        {
            
        }

        public string PlatformName => PlatformNames.GitHub;

        public void SetConfig(GitConfig gitConfig)
        {
            _config = gitConfig;
        }

        private void SetDefaultHeaders(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.Token}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
        }

        public async Task<List<GitRepo>> GetAllReposAsync()
        {
            var gitRepos = new List<GitRepo>();
            using (var httpClient = new HttpClient())
            {
                SetDefaultHeaders(httpClient);

                var response = httpClient.GetAsync($"{_config.APIBaseURL}/users/{_config.Owner}/repos?per_page=1000").Result;

                var data = await response.Content.ReadAsStringAsync();

                var items = JsonSerializer.Deserialize<JsonArray>(data);

                foreach(var item in items)
                {
                    var gitRepo = new GitRepo()
                    {
                        Name = (string)item["name"],
                        URL = (string)item["url"]
                    };
                    gitRepos.Add(gitRepo);
                }              
            }
            return gitRepos.OrderBy(r => r.Name).ToList();
        }
   
        public async Task DownloadRepo(string name, IFileStorage fileStorage)
        {            
            using (var httpClient = new HttpClient())
            {
                SetDefaultHeaders(httpClient);

                var response = httpClient.GetAsync($"{_config.APIBaseURL}/repos/{_config.Owner}/{name}/contents").Result;

                var data = await response.Content.ReadAsStringAsync();

                var items = JsonSerializer.Deserialize<JsonArray>(data);                

                // Download each file/folder
                foreach (var item in items)
                {
                    switch ((string)item["type"])
                    {
                        case "file":
                            var fileName = (string)item["name"];
                            await DownloadFile((string)item["download_url"], new string[0], fileName, fileStorage);
                            break;
                        case "dir":
                            var folderName = (string)item["name"];
                            await DownloadFolder((string)item["url"], new[] { folderName }, fileStorage);
                            break;
                    }
                }                
            }
        }

        /// <summary>
        /// Downloads file to file storage
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="folderNames"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStorage"></param>
        /// <returns></returns>
        private async Task DownloadFile(string remoteUrl, string[] folderNames, string fileName, IFileStorage fileStorage)
        {
            using (var httpClient = new HttpClient())
            {
                SetDefaultHeaders(httpClient);

                var response = httpClient.GetAsync(remoteUrl).Result;

                // Read the content into a MemoryStream and then write to file
                using (var memoryStream = await response.Content.ReadAsStreamAsync())
                {
                    var content = new byte[memoryStream.Length];
                    memoryStream.Read(content, 0, content.Length);

                    await fileStorage.WriteFileAsync(folderNames, fileName, content);
                }
            }
        }

        /// <summary>
        /// Downloads folder to file storage
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="folderNames"></param>
        /// <param name="fileStorage"></param>
        /// <returns></returns>
        private async Task DownloadFolder(string remoteUrl, string[] folderNames, IFileStorage fileStorage)
        {            
            using (var httpClient = new HttpClient())
            {
                SetDefaultHeaders(httpClient);

                var response = httpClient.GetAsync(remoteUrl).Result;

                var data = await response.Content.ReadAsStringAsync();

                var items = JsonSerializer.Deserialize<JsonArray>(data);

                foreach (var item in items)
                {
                    switch ((string)item["type"])
                    {
                        case "file":
                            var fileName = (string)item["name"];

                            await DownloadFile((string)item["download_url"], folderNames, fileName, fileStorage);
                            break;
                        case "dir":
                            var folderName = (string)item["name"];

                            // Pass this folder & parent folders
                            var folderNamesCopy = (string[])folderNames.Clone();                            
                            Array.Resize(ref folderNamesCopy, folderNamesCopy.Length + 1);
                            folderNamesCopy[folderNamesCopy.Length - 1] = folderName;

                            await DownloadFolder((string)item["url"], folderNamesCopy, fileStorage);
                            break;
                    }
                }
            }
        }
    }
}

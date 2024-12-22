using CFGitBackup.Constants;
using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        public async Task DownloadRepo(string name, string localFolder)
        {
            Directory.CreateDirectory(localFolder);

            using (var httpClient = new HttpClient())
            {
                SetDefaultHeaders(httpClient);
                
                var response = httpClient.GetAsync($"{_config.APIBaseURL}/repos/{_config.Owner}/{name}/contents").Result;

                var data = await response.Content.ReadAsStringAsync();

                var items = JsonSerializer.Deserialize<JsonArray>(data);

                // Download each file/folder
                foreach(var item in items)
                {                    
                    switch((string)item["type"])
                    {
                        case "file":
                            var file = Path.Combine(localFolder, (string)item["name"]);
                            await DownloadFile((string)item["download_url"], file);
                            break;
                        case "dir":
                            var folder = Path.Combine(localFolder, (string)item["name"]);
                            await DownloadFolder((string)item["url"], folder);
                            break;                   
                    }
                }

                int zzz = 1000;
            }            
        }

        /// <summary>
        /// Downloads file
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="localFile"></param>
        /// <returns></returns>
        private async Task DownloadFile(string remoteUrl, string localFile)
        {
            using (var httpClient = new HttpClient())
            {
                SetDefaultHeaders(httpClient);

                var response = httpClient.GetAsync(remoteUrl).Result;

                // Read the content into a MemoryStream and then write to file
                using (var memoryStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = File.Create(localFile))
                    {
                        //await memoryStream.CopyToAsync(fileStream);   // Hangs
                        memoryStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }                    
                }                
            }
        }

        /// <summary>
        /// Downloads folder
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="localFolder"></param>
        /// <returns></returns>
        private async Task DownloadFolder(string remoteUrl, string localFolder)
        {
            Directory.CreateDirectory(localFolder);

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
                            var file = Path.Combine(localFolder, (string)item["name"]);
                            await DownloadFile((string)item["download_url"], file);
                            break;
                        case "dir":
                            var folder = Path.Combine(localFolder, (string)item["name"]);
                            await DownloadFolder((string)item["url"], folder);
                            break;                     
                    }
                }
            }
        }
    }
}

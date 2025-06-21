using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace HelloGit.Api.Services
{
    public class GitHubApiClient
    {
        private readonly HttpClient _httpClient;

        public GitHubApiClient(string token)
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("token", token);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HelloGit-App");
        }

        public async Task<List<GitHubRepo>> GetRepositoriesAsync(string keyword, int maxResults = 200)
        {
            var repos = new List<GitHubRepo>();
            int perPage = 100;
            int pages = (int)Math.Ceiling(maxResults / (double)perPage);

            for (int page = 1; page <= pages; page++)
            {
                var url = $"https://api.github.com/search/repositories?q={keyword}+in:name&sort=stars&order=desc&per_page={perPage}&page={page}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<SearchRepositoriesResponse>();

                if (result?.Items != null)
                {
                    repos.AddRange(result.Items);
                }

                if (repos.Count >= maxResults)
                    break;
            }

            return repos.Take(maxResults).ToList();
        }

        public async Task<int> GetContributorsCountAsync(string owner, string repo)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page=1&anon=true";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return 0;

            if (response.Headers.TryGetValues("Link", out var linkHeaders))
            {
                var linkHeader = linkHeaders.FirstOrDefault() ?? "";
                var match = Regex.Match(linkHeader, @"&page=(\d+)>; rel=""last""");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int lastPage))
                    return lastPage;
            }

            var contributorsJson = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(contributorsJson) ? 0 : 1;
        }

        public async Task<List<GitHubIssue>> GetLastOpenIssuesAsync(string owner, string repoName, int maxCount)
        {
            string url = $"https://api.github.com/repos/{owner}/{repoName}/issues?state=open&per_page={maxCount}&sort=created&direction=desc";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<GitHubIssue>();

            var json = await response.Content.ReadAsStringAsync();
            var issues = JsonSerializer.Deserialize<List<GitHubIssue>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return issues ?? new List<GitHubIssue>();
        }

        public class SearchRepositoriesResponse
        {
            [JsonPropertyName("total_count")]
            public int TotalCount { get; set; }

            [JsonPropertyName("items")]
            public List<GitHubRepo> Items { get; set; } = new();
        }

        public class GitHubRepo
        {
            [JsonPropertyName("full_name")]
            public string FullName { get; set; } = null!;

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("html_url")]
            public string HtmlUrl { get; set; } = null!;

            [JsonPropertyName("stargazers_count")]
            public int StargazersCount { get; set; }

            [JsonPropertyName("open_issues_count")]
            public int OpenIssuesCount { get; set; }
        }

        public class GitHubIssue
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = null!;

            [JsonPropertyName("number")]
            public int IssueNumber { get; set; }

            [JsonPropertyName("html_url")]
            public string HtmlUrl { get; set; } = null!;

            [JsonPropertyName("created_at")]
            public DateTime CreatedAt { get; set; }
        }
    }
}
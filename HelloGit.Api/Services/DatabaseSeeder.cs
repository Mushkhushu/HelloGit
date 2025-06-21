
using HelloGit.Api.Data;
using HelloGit.Api.Models;

namespace HelloGit.Api.Services
{
    public class DatabaseSeeder
    {
        private readonly GitHubApiClient _gitHubApiClient;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="gitHubApiClient">An instance of <see cref="GitHubApiClient"/> used to fetch data from the GitHub API.</param>
        /// <param name="dbContext">The database context <see cref="AppDbContext"/> used to interact with the SQLite database.</param>
        public DatabaseSeeder(GitHubApiClient gitHubApiClient, AppDbContext dbContext)
        {
            _gitHubApiClient = gitHubApiClient;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Seeds the database by fetching repositories and their recent issues from GitHub,
        /// then inserts or updates the repository and issue data into the local SQLite database.
        /// </summary>
        /// <remarks>
        /// For each repository fetched by the GitHub API:
        /// - It updates existing repository records or adds new ones.
        /// - Retrieves the contributors count.
        /// - Fetches the last 5 open issues and replaces existing issues for that repository in the database.
        /// Finally, changes are saved to the database asynchronously.
        public async Task SeedAsync()
        {
            var reposFromApi = await _gitHubApiClient.GetRepositoriesAsync("json", 200);

            foreach (var apiRepo in reposFromApi)
            {
                var split = apiRepo.FullName.Split('/');
                if (split.Length != 2) continue;

                string owner = split[0];
                string repoName = split[1];

                var contributorsCount = await _gitHubApiClient.GetContributorsCountAsync(owner, repoName);

                var repositoryEntity = _dbContext.Repositories.SingleOrDefault(r => r.FullName == apiRepo.FullName);
                if (repositoryEntity != null)
                {
                    repositoryEntity.Description = apiRepo.Description;
                    repositoryEntity.HtmlUrl = apiRepo.HtmlUrl;
                    repositoryEntity.Stars = apiRepo.StargazersCount;
                    repositoryEntity.OpenIssuesCount = apiRepo.OpenIssuesCount;
                    repositoryEntity.ContributorsCount = contributorsCount;
                }
                else
                {
                    repositoryEntity = new Repository
                    {
                        FullName = apiRepo.FullName,
                        Description = apiRepo.Description,
                        HtmlUrl = apiRepo.HtmlUrl,
                        Stars = apiRepo.StargazersCount,
                        OpenIssuesCount = apiRepo.OpenIssuesCount,
                    };

                    _dbContext.Repositories.Add(repositoryEntity);
                }
                var lastFiveIssues = await _gitHubApiClient.GetLastOpenIssuesAsync(owner, repoName, 5);

                var existingIssues = _dbContext.Issues.Where(i => i.RepositoryId == repositoryEntity.Id);
                _dbContext.Issues.RemoveRange(existingIssues);

                foreach (var apiIssue in lastFiveIssues)
                {
                    var issue = new Issue
                    {
                        Repository = repositoryEntity,
                        Title = apiIssue.Title,
                        HtmlUrl = apiIssue.HtmlUrl,
                        CreatedAt = apiIssue.CreatedAt,
                        IssueNumber = apiIssue.IssueNumber
                    };
                    _dbContext.Issues.Add(issue);
                }
            }
            await _dbContext.SaveChangesAsync();
        }

    }
}
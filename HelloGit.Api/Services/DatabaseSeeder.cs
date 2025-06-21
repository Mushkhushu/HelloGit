
using HelloGit.Api.Data;
using HelloGit.Api.Models;
using CsvHelper;
using System.Globalization;


namespace HelloGit.Api.Services
{
    public class DatabaseSeeder
    {
        private readonly GitHubApiClient? _gitHubApiClient;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="gitHubApiClient">
        /// An optional instance of <see cref="GitHubApiClient"/> used to fetch data from the GitHub API. 
        /// Can be null when seeding from CSV files.
        /// </param>
        /// <param name="dbContext">The database context <see cref="AppDbContext"/> used to interact with the SQLite database.</param>
        public DatabaseSeeder(GitHubApiClient? gitHubApiClient, AppDbContext dbContext)
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
            Console.WriteLine("Seeding from GitHub API.");
            if (_gitHubApiClient == null)
                throw new InvalidOperationException("GitHubApiClient is required to seed from API.");

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

        /// <summary>
        /// Seeds the database by importing repository and issue data from CSV files.
        /// </summary>
        /// <remarks>
        /// This method performs the following steps:
        /// - Clears all existing repositories and issues from the database.
        /// - Reads repository data from the "SeedData/repositories.csv" file and inserts it into the database.
        /// - Reads issue data from the "SeedData/issues.csv" file and inserts issues linked to their repositories.
        /// - Saves changes to the database after repositories and issues are inserted.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SeedFromCsvAsync()
        {
            Console.WriteLine("Seeding from .csv files.");
            // Clear existing data
            _dbContext.Issues.RemoveRange(_dbContext.Issues);
            _dbContext.Repositories.RemoveRange(_dbContext.Repositories);
            await _dbContext.SaveChangesAsync();

            // Seed repositories
            using var repoReader = new StreamReader("SeedData/repositories.csv");
            using var csvRepo = new CsvReader(repoReader, CultureInfo.InvariantCulture);

            var repoRecords = csvRepo.GetRecords<RepositoryCsvModel>().ToList();

            foreach (var repoRecord in repoRecords)
            {
                var repo = new Repository
                {
                    FullName = repoRecord.FullName,
                    Description = repoRecord.Description,
                    HtmlUrl = repoRecord.HtmlUrl,
                    Stars = repoRecord.Stars,
                    ContributorsCount = repoRecord.ContributorsCount,
                    OpenIssuesCount = repoRecord.OpenIssuesCount
                };
                _dbContext.Repositories.Add(repo);
            }

            await _dbContext.SaveChangesAsync();

            // Seed issues
            using var issueReader = new StreamReader("SeedData/issues.csv");
            using var csvIssue = new CsvReader(issueReader, CultureInfo.InvariantCulture);

            var issueRecords = csvIssue.GetRecords<IssueCsvModel>().ToList();

            foreach (var issueRecord in issueRecords)
            {
                var repo = _dbContext.Repositories.SingleOrDefault(r => r.Id == issueRecord.RepositoryId);
                if (repo == null) continue;

                var issue = new Issue
                {
                    Repository = repo,
                    Title = issueRecord.Title,
                    HtmlUrl = issueRecord.HtmlUrl,
                    CreatedAt = issueRecord.CreatedAt,
                    IssueNumber = issueRecord.IssueNumber
                };
                _dbContext.Issues.Add(issue);
            }

            await _dbContext.SaveChangesAsync();
        }

        // Auxiliary classes for CSV mapping
        private class RepositoryCsvModel
        {
            public string FullName { get; set; } = null!;
            public string? Description { get; set; }
            public string HtmlUrl { get; set; } = null!;
            public int Stars { get; set; }
            public int ContributorsCount { get; set; }
            public int OpenIssuesCount { get; set; }
        }

        private class IssueCsvModel
        {
            public int RepositoryId { get; set; }
            public int IssueNumber { get; set; }
            public string Title { get; set; } = null!;
            public string HtmlUrl { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
        }
    }
}
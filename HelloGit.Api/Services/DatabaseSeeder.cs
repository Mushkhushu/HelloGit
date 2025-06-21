
using HelloGit.Api.Data;
using HelloGit.Api.Models;

namespace HelloGit.Api.Services
{
    public class DatabaseSeeder
    {
        private readonly GitHubApiClient _gitHubApiClient;
        private readonly AppDbContext _dbContext;

        public DatabaseSeeder(GitHubApiClient gitHubApiClient, AppDbContext dbContext)
        {
            _gitHubApiClient = gitHubApiClient;
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            var reposFromApi = await _gitHubApiClient.GetRepositoriesAsync("json", 200);

            foreach (var apiRepo in reposFromApi)
            {
                var repositoryEntity = _dbContext.Repositories.SingleOrDefault(r => r.FullName == apiRepo.FullName);
                if (repositoryEntity != null)
                {
                    repositoryEntity.Description = apiRepo.Description;
                    repositoryEntity.HtmlUrl = apiRepo.HtmlUrl;
                    repositoryEntity.Stars = apiRepo.StargazersCount;
                    repositoryEntity.OpenIssuesCount = apiRepo.OpenIssuesCount;
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
            }
            await _dbContext.SaveChangesAsync();
        }

    }
}
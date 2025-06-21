
using Microsoft.Extensions.Configuration;
using HelloGit.Api.Services;
using HelloGit.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelloGit.Api
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                // Github token is needed to access the GitHub API
                // in appsettings.Development.json, you should have a line like this:
                // Must be like : "GitHubToken": "your-token-here"
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            string gitHubToken = configuration["GitHubToken"];
            if (string.IsNullOrWhiteSpace(gitHubToken))
            {
                Console.WriteLine("GitHub token is missing in configuration. Please set it in appsettings.Development.json.");
            }

            var gitHubClient = new GitHubApiClient(gitHubToken);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite("Data Source=hellogit.db");

            using var dbContext = new AppDbContext(optionsBuilder.Options);
            await dbContext.Database.EnsureCreatedAsync();

            var seeder = new DatabaseSeeder(gitHubClient, dbContext);
            // Change line with await seeder.SeedAsync() to get the data from the API with a GitHub token
            await seeder.SeedFromCsvAsync();


            Console.WriteLine("Seed terminé.");
        }
    }
}
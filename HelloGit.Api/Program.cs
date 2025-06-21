
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
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            string gitHubToken = configuration["GitHubToken"];

            var gitHubClient = new GitHubApiClient(gitHubToken);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite("Data Source=hellogit.db");

            using var dbContext = new AppDbContext(optionsBuilder.Options);
            await dbContext.Database.EnsureCreatedAsync();

            var seeder = new DatabaseSeeder(gitHubClient, dbContext);
            await seeder.SeedAsync();


            Console.WriteLine("Seed terminé.");
        }
    }
}
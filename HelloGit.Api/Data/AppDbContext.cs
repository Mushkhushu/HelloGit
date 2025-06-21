using Microsoft.EntityFrameworkCore;
using HelloGit.Api.Models;

namespace HelloGit.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Issue> Issues { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Repository>()
                .HasMany(r => r.LastFiveIssues)
                .WithOne(i => i.Repository)
                .HasForeignKey(i => i.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
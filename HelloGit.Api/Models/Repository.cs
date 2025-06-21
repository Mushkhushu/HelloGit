using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HelloGit.Api.Models
{
    /// <summary>
    /// Represents a GitHub repository with relevant details.
    /// </summary>
    public class Repository
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Full name of the repository (e.g. owner/repo).
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Repository description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// URL to the repository page on GitHub.
        /// </summary>
        public string HtmlUrl { get; set; } = null!;

        /// <summary>
        /// Number of stars (stargazers).
        /// </summary>
        public int Stars { get; set; }

        /// <summary>
        /// Number of contributors.
        /// </summary>
        public int ContributorsCount { get; set; }

        /// <summary>
        /// Number of open issues.
        /// </summary>
        public int OpenIssuesCount { get; set; }

        /// <summary>
        /// Navigation property for last 5 open issues.
        /// </summary>
        public List<Issue> LastFiveIssues { get; set; } = new();
    }
}
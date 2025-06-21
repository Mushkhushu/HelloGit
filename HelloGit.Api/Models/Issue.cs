using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelloGit.Api.Models
{
    /// <summary>
    /// Represents an issue of a repository.
    /// </summary>
    public class Issue
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the related repository.
        /// </summary>
        public int RepositoryId { get; set; }

        [ForeignKey(nameof(RepositoryId))]
        public Repository Repository { get; set; } = null!;

        /// <summary>
        /// Number of the issue on GitHub (unique per repository).
        /// </summary>
        public int IssueNumber { get; set; }

        /// <summary>
        /// Title of the issue.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// URL to the issue page on GitHub.
        /// </summary>
        public string HtmlUrl { get; set; } = null!;

        /// <summary>
        /// Creation date of the issue.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelloGit.Api.Models;
using HelloGit.Api.Data; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelloGit.Web.Pages.Repositories
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Repository> Repositories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Repositories = await _dbContext.Repositories.ToListAsync();
        }
    }
}
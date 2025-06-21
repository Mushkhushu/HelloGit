using Microsoft.AspNetCore.Mvc.RazorPages;
using HelloGit.Api.Models;
using HelloGit.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HelloGit.Web.Pages.Repositories
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public DetailsModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Repository? Repository { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == 0)
                return NotFound();

            Repository = await _dbContext.Repositories
                .Include(r => r.LastFiveIssues)
                .FirstOrDefaultAsync(r => r.Id == Id);

            if (Repository == null)
                return NotFound();

            return Page();
        }
    }
}
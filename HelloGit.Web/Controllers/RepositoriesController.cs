using HelloGit.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace HelloGit.Web.Controllers
{
    public class RepositoriesController : Controller
    {
        private readonly AppDbContext _db;

        public RepositoriesController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var repositories = _db.Repositories
                .OrderByDescending(r => r.Stars)
                .ToList();
            return View(repositories);
        }

        public IActionResult Details(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = _db.Repositories
                .Include(r => r.LastFiveIssues)
                .FirstOrDefault(r => r.Id == id);

            if (repository == null)
                return NotFound();

            return View(repository);
        }
    }
}
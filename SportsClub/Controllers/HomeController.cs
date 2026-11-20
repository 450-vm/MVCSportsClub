using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;

namespace SportsClub.Controllers;

public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.TrainerCount = await db.Trainers.CountAsync();
        ViewBag.MemberCount = await db.Members.CountAsync();
        return View(await db.TrainingSessions.AsNoTracking().Include(x => x.Trainer)
            .Include(x => x.Enrollments).Where(x => x.StartDateTime > DateTime.UtcNow)
            .OrderBy(x => x.StartDateTime).Take(3).ToListAsync());
    }
    public IActionResult About() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
    public IActionResult Status(int code)
    {
        Response.StatusCode = code;
        ViewBag.Code = code;
        return View();
    }
}

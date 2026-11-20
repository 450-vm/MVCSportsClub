using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;

namespace SportsClub.Controllers;

public abstract class ClubController(ILogger logger) : Controller
{
    protected async Task<bool> SaveAsync(ApplicationDbContext db)
    {
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "A club record could not be saved.");
            ModelState.AddModelError("", "The change could not be saved. A related record may have changed; refresh and try again.");
            return false;
        }
    }
    protected void Success(string message) => TempData["Success"] = message;
}

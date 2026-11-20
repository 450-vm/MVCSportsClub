using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;
using SportsClub.Models;
using SportsClub.Services;
using SportsClub.ViewModels;

namespace SportsClub.Controllers;

public class TrainingSessionsController(ApplicationDbContext db, EnrollmentService enrollments,
    ILogger<TrainingSessionsController> logger) : ClubController(logger)
{
    public async Task<IActionResult> Index() => View(await Sessions().OrderBy(x => x.StartDateTime).ToListAsync());
    public async Task<IActionResult> Schedule() => View(await Sessions().Where(x => x.StartDateTime > DateTime.UtcNow)
        .OrderBy(x => x.StartDateTime).ToListAsync());
    private IQueryable<TrainingSession> Sessions() => db.TrainingSessions.AsNoTracking().Include(x => x.Trainer).Include(x => x.Enrollments);
    public async Task<IActionResult> Details(int id)
    {
        var session = await Sessions().Include(x => x.Enrollments).ThenInclude(x => x.Member).SingleOrDefaultAsync(x => x.Id == id);
        if (session is null) return NotFound();
        var enrolledIds = session.Enrollments.Select(x => x.MemberId).ToArray();
        ViewBag.Members = await db.Members.AsNoTracking().Where(x => !enrolledIds.Contains(x.Id))
            .OrderBy(x => x.LastName).Select(x => new SelectListItem(x.FirstName + " " + x.LastName + " (" + x.Email + ")", x.Id.ToString())).ToListAsync();
        return View(session);
    }
    public async Task<IActionResult> Create() => View(await WithTrainers(new SessionInput()));
    [HttpPost]
    public async Task<IActionResult> Create(SessionInput input)
    {
        var start = await ValidateInput(input);
        if (start <= DateTime.UtcNow) ModelState.AddModelError(nameof(input.StartLocal), "The session must start in the future.");
        if (!ModelState.IsValid) return View(await WithTrainers(input));
        var session = new TrainingSession
        {
            Title = input.Title.Trim(), Description = input.Description.Trim(), TrainerId = input.TrainerId!.Value,
            StartDateTime = start!.Value, DurationMinutes = input.DurationMinutes, Capacity = input.Capacity
        };
        var result = await enrollments.CreateSessionAsync(session);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(await WithTrainers(input));
        }
        Success(result.Message);
        return RedirectToAction(nameof(Details), new { id = session.Id });
    }
    public async Task<IActionResult> Edit(int id)
    {
        var session = await db.TrainingSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (session is null) return NotFound();
        return View(await WithTrainers(new SessionInput
        {
            Title = session.Title, Description = session.Description, TrainerId = session.TrainerId,
            StartLocal = ClubTime.ToLocal(session.StartDateTime), DurationMinutes = session.DurationMinutes, Capacity = session.Capacity
        }));
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, SessionInput input)
    {
        if (!await db.TrainingSessions.AnyAsync(x => x.Id == id)) return NotFound();
        var start = await ValidateInput(input);
        if (!ModelState.IsValid) return View(await WithTrainers(input));
        var result = await enrollments.UpdateSessionAsync(id, input, start!.Value);
        if (result.NotFound) return NotFound();
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(await WithTrainers(input));
        }
        Success(result.Message);
        return RedirectToAction(nameof(Details), new { id });
    }
    public async Task<IActionResult> Delete(int id)
    {
        var session = await Sessions().SingleOrDefaultAsync(x => x.Id == id);
        return session is null ? NotFound() : View(session);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var session = await Sessions().SingleOrDefaultAsync(x => x.Id == id);
        if (session is null) return NotFound();
        db.TrainingSessions.Remove(session);
        if (!await SaveAsync(db)) return View("Delete", session);
        Success("Training session and associated enrollments deleted.");
        return RedirectToAction(nameof(Index));
    }
    private async Task<SessionInput> WithTrainers(SessionInput input)
    {
        input.Trainers = await db.Trainers.AsNoTracking().OrderBy(x => x.LastName)
            .Select(x => new SelectListItem(x.FirstName + " " + x.LastName + " — " + x.Specialization, x.Id.ToString())).ToListAsync();
        return input;
    }
    private async Task<DateTime?> ValidateInput(SessionInput input)
    {
        if (input.TrainerId.HasValue && !await db.Trainers.AnyAsync(x => x.Id == input.TrainerId))
            ModelState.AddModelError(nameof(input.TrainerId), "Select an existing trainer.");
        if (!input.StartLocal.HasValue) return null;
        if (!ClubTime.TryToUtc(input.StartLocal.Value, out var utc))
        {
            ModelState.AddModelError(nameof(input.StartLocal), "This time is ambiguous or unavailable due to daylight saving. Choose another time.");
            return null;
        }
        return utc;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;
using SportsClub.Models;
using SportsClub.ViewModels;

namespace SportsClub.Controllers;

public class TrainersController(ApplicationDbContext db, ILogger<TrainersController> logger) : ClubController(logger)
{
    public async Task<IActionResult> Index() => View(await db.Trainers.AsNoTracking()
        .OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync());
    public async Task<IActionResult> Details(int id)
    {
        var trainer = await db.Trainers.AsNoTracking().Include(x => x.TrainingSessions).SingleOrDefaultAsync(x => x.Id == id);
        return trainer is null ? NotFound() : View(trainer);
    }
    public IActionResult Create() => View(new TrainerInput());
    [HttpPost]
    public async Task<IActionResult> Create(TrainerInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var trainer = new Trainer();
        Apply(trainer, input);
        db.Trainers.Add(trainer);
        if (!await SaveAsync(db)) return View(input);
        Success("Trainer created.");
        return RedirectToAction(nameof(Details), new { id = trainer.Id });
    }
    public async Task<IActionResult> Edit(int id)
    {
        var trainer = await db.Trainers.FindAsync(id);
        return trainer is null ? NotFound() : View(new TrainerInput
        {
            FirstName = trainer.FirstName, LastName = trainer.LastName,
            Email = trainer.Email, PhoneNumber = trainer.PhoneNumber, Specialization = trainer.Specialization
        });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, TrainerInput input)
    {
        var trainer = await db.Trainers.FindAsync(id);
        if (trainer is null) return NotFound();
        if (!ModelState.IsValid) return View(input);
        Apply(trainer, input);
        if (!await SaveAsync(db)) return View(input);
        Success("Trainer updated.");
        return RedirectToAction(nameof(Details), new { id });
    }
    public async Task<IActionResult> Delete(int id)
    {
        var trainer = await db.Trainers.AsNoTracking().Include(x => x.TrainingSessions).SingleOrDefaultAsync(x => x.Id == id);
        return trainer is null ? NotFound() : View(trainer);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var trainer = await db.Trainers.Include(x => x.TrainingSessions).SingleOrDefaultAsync(x => x.Id == id);
        if (trainer is null) return NotFound();
        if (trainer.TrainingSessions.Count > 0)
        {
            ModelState.AddModelError("", "Reassign or delete this trainer's sessions before deleting the trainer.");
            return View("Delete", trainer);
        }
        db.Trainers.Remove(trainer);
        if (!await SaveAsync(db)) return View("Delete", trainer);
        Success("Trainer deleted.");
        return RedirectToAction(nameof(Index));
    }
    private static void Apply(Trainer trainer, TrainerInput input)
    {
        trainer.FirstName = input.FirstName.Trim(); trainer.LastName = input.LastName.Trim();
        trainer.Email = input.Email.Trim(); trainer.PhoneNumber = input.PhoneNumber.Trim();
        trainer.Specialization = input.Specialization.Trim();
    }
}

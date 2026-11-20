using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;
using SportsClub.Models;
using SportsClub.ViewModels;

namespace SportsClub.Controllers;

public class MembersController(ApplicationDbContext db, ILogger<MembersController> logger) : ClubController(logger)
{
    public async Task<IActionResult> Index() => View(await db.Members.AsNoTracking()
        .OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync());
    public async Task<IActionResult> Details(int id)
    {
        var member = await db.Members.AsNoTracking().Include(x => x.Enrollments)
            .ThenInclude(x => x.TrainingSession).ThenInclude(x => x.Trainer).SingleOrDefaultAsync(x => x.Id == id);
        return member is null ? NotFound() : View(member);
    }
    public IActionResult Create() => View(new MemberInput());
    [HttpPost]
    public async Task<IActionResult> Create(MemberInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var member = new Member(); Apply(member, input);
        db.Members.Add(member);
        if (!await SaveAsync(db)) return View(input);
        Success("Member created.");
        return RedirectToAction(nameof(Details), new { id = member.Id });
    }
    public async Task<IActionResult> Edit(int id)
    {
        var member = await db.Members.FindAsync(id);
        return member is null ? NotFound() : View(new MemberInput
        {
            FirstName = member.FirstName, LastName = member.LastName,
            Email = member.Email, PhoneNumber = member.PhoneNumber, JoinDate = member.JoinDate
        });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, MemberInput input)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null) return NotFound();
        if (!ModelState.IsValid) return View(input);
        Apply(member, input);
        if (!await SaveAsync(db)) return View(input);
        Success("Member updated.");
        return RedirectToAction(nameof(Details), new { id });
    }
    public async Task<IActionResult> Delete(int id)
    {
        var member = await db.Members.AsNoTracking().Include(x => x.Enrollments).SingleOrDefaultAsync(x => x.Id == id);
        return member is null ? NotFound() : View(member);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var member = await db.Members.Include(x => x.Enrollments).SingleOrDefaultAsync(x => x.Id == id);
        if (member is null) return NotFound();
        db.Members.Remove(member);
        if (!await SaveAsync(db)) return View("Delete", member);
        Success("Member and associated enrollments deleted.");
        return RedirectToAction(nameof(Index));
    }
    private static void Apply(Member member, MemberInput input)
    {
        member.FirstName = input.FirstName.Trim(); member.LastName = input.LastName.Trim();
        member.Email = input.Email.Trim(); member.PhoneNumber = input.PhoneNumber.Trim();
        member.JoinDate = input.JoinDate!.Value;
    }
}

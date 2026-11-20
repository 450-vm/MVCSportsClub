using Microsoft.AspNetCore.Mvc;
using SportsClub.Services;

namespace SportsClub.Controllers;

public class EnrollmentsController(EnrollmentService enrollments) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Create(int sessionId, int memberId)
    {
        if (!ModelState.IsValid || sessionId <= 0 || memberId <= 0) return BadRequest();
        var result = await enrollments.EnrollAsync(sessionId, memberId);
        if (result.NotFound) return NotFound();
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction("Details", "TrainingSessions", new { id = sessionId });
    }
    [HttpPost]
    public async Task<IActionResult> Cancel(int sessionId, int memberId)
    {
        if (!ModelState.IsValid || sessionId <= 0 || memberId <= 0) return BadRequest();
        var result = await enrollments.CancelAsync(sessionId, memberId);
        if (result.NotFound) return NotFound();
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction("Details", "TrainingSessions", new { id = sessionId });
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;
using SportsClub.Models;
using SportsClub.ViewModels;

namespace SportsClub.Services;

public record OperationResult(bool Success, string Message, bool NotFound = false);

public class EnrollmentService(ApplicationDbContext db, ILogger<EnrollmentService> logger)
{
    // Reserve the SQLite writer before checking capacity or schedules. Session
    // creation, edits and enrollments must all read after obtaining this lock.
    private async Task<OperationResult> WriteAsync(Func<Task<OperationResult>> operation)
    {
        try
        {
            await db.Database.OpenConnectionAsync();
            var connection = (SqliteConnection)db.Database.GetDbConnection();
            using var transaction = connection.BeginTransaction(deferred: false);
            await using var enlisted = await db.Database.UseTransactionAsync(transaction);
            var result = await operation();
            if (result.Success)
            {
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            return result;
        }
        catch (Exception ex) when (ex is DbUpdateException or SqliteException)
        {
            logger.LogWarning(ex, "A club enrollment or session update could not be saved.");
            return new(false, "The change could not be saved. Another change may have occurred; refresh and try again.");
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    // Strict inequalities allow adjacent sessions, while catching partial overlap
    // and either session completely containing the other. All times are UTC.
    private IQueryable<TrainingSession> OverlappingSessions(DateTime startUtc, int durationMinutes, int? excludingId = null)
    {
        var endUtc = startUtc.AddMinutes(durationMinutes);
        return db.TrainingSessions.Where(x => x.Id != excludingId &&
            x.StartDateTime < endUtc && x.StartDateTime.AddMinutes(x.DurationMinutes) > startUtc);
    }

    public Task<OperationResult> CreateSessionAsync(TrainingSession session) => WriteAsync(async () =>
    {
        if (!await db.Trainers.AnyAsync(x => x.Id == session.TrainerId))
            return new(false, "Select an existing trainer.");
        if (session.StartDateTime <= DateTime.UtcNow)
            return new(false, "The session must start in the future.");
        if (await OverlappingSessions(session.StartDateTime, session.DurationMinutes)
                .AnyAsync(x => x.TrainerId == session.TrainerId))
            return new(false, "This trainer already has a session that overlaps with this time.");
        db.TrainingSessions.Add(session);
        return new(true, "Training session created.");
    });

    public Task<OperationResult> EnrollAsync(int sessionId, int memberId) => WriteAsync(async () =>
    {
        var session = await db.TrainingSessions.FindAsync(sessionId);
        if (session is null || !await db.Members.AnyAsync(x => x.Id == memberId))
            return new(false, "The session or member no longer exists.", true);
        if (session.StartDateTime <= DateTime.UtcNow)
            return new(false, "Enrollment is closed because this session has already started.");
        if (await db.TrainingEnrollments.AnyAsync(x => x.TrainingSessionId == sessionId && x.MemberId == memberId))
            return new(false, "This member is already enrolled in the session.");
        if (await db.TrainingEnrollments.CountAsync(x => x.TrainingSessionId == sessionId) >= session.Capacity)
            return new(false, "This session is full.");
        if (await OverlappingSessions(session.StartDateTime, session.DurationMinutes, sessionId)
                .AnyAsync(x => x.Enrollments.Any(e => e.MemberId == memberId)))
            return new(false, "This member is already enrolled in another session that overlaps with this time.");
        db.TrainingEnrollments.Add(new TrainingEnrollment { TrainingSessionId = sessionId, MemberId = memberId });
        return new(true, "Member enrolled successfully.");
    });

    public Task<OperationResult> CancelAsync(int sessionId, int memberId) => WriteAsync(async () =>
    {
        var enrollment = await db.TrainingEnrollments.FindAsync(memberId, sessionId);
        if (enrollment is null) return new(false, "This enrollment no longer exists.", true);
        db.TrainingEnrollments.Remove(enrollment);
        return new(true, "Enrollment cancelled.");
    });

    public Task<OperationResult> UpdateSessionAsync(int id, SessionInput input, DateTime startUtc) => WriteAsync(async () =>
    {
        var session = await db.TrainingSessions.FindAsync(id);
        if (session is null) return new(false, "This session no longer exists.", true);
        if (!await db.Trainers.AnyAsync(x => x.Id == input.TrainerId))
            return new(false, "Select an existing trainer.");
        if (startUtc != session.StartDateTime && startUtc <= DateTime.UtcNow)
            return new(false, "A changed start time must be in the future.");
        var count = await db.TrainingEnrollments.CountAsync(x => x.TrainingSessionId == id);
        if (input.Capacity < count)
            return new(false, $"Capacity cannot be less than the {count} enrolled members.");
        var overlapping = OverlappingSessions(startUtc, input.DurationMinutes, id);
        if (await overlapping.AnyAsync(x => x.TrainerId == input.TrainerId))
            return new(false, "This trainer already has a session that overlaps with this time.");
        var enrolledMemberIds = db.TrainingEnrollments.Where(x => x.TrainingSessionId == id).Select(x => x.MemberId);
        if (await overlapping.AnyAsync(x => x.Enrollments.Any(e => enrolledMemberIds.Contains(e.MemberId))))
            return new(false, "An enrolled member already has another session that overlaps with this time.");
        session.Title = input.Title.Trim();
        session.Description = input.Description.Trim();
        session.TrainerId = input.TrainerId!.Value;
        session.StartDateTime = startUtc;
        session.DurationMinutes = input.DurationMinutes;
        session.Capacity = input.Capacity;
        return new(true, "Training session updated.");
    });
}

using Microsoft.EntityFrameworkCore;
using SportsClub.Models;
using SportsClub.Services;

namespace SportsClub.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // Seed a fresh database only. Re-running never overwrites user changes.
        if (await db.Trainers.AnyAsync() || await db.Members.AnyAsync() || await db.TrainingSessions.AnyAsync()) return;
        var trainers = new[]
        {
            new Trainer { FirstName = "Elena", LastName = "Petrova", Specialization = "Yoga & mobility", Email = "elena@example.com", PhoneNumber = "+359888100101" },
            new Trainer { FirstName = "Ivan", LastName = "Dimitrov", Specialization = "Strength & conditioning", Email = "ivan@example.com", PhoneNumber = "+359888100102" },
            new Trainer { FirstName = "Maria", LastName = "Georgieva", Specialization = "Cardio & endurance", Email = "maria@example.com", PhoneNumber = "+359888100103" }
        };
        var members = new[]
        {
            new Member { FirstName = "Alex", LastName = "Ivanov", Email = "alex@example.com", PhoneNumber = "+359888200101", JoinDate = ClubTime.Today.AddDays(-30) },
            new Member { FirstName = "Sofia", LastName = "Todorova", Email = "sofia@example.com", PhoneNumber = "+359888200102", JoinDate = ClubTime.Today.AddDays(-14) },
            new Member { FirstName = "Nikolay", LastName = "Kolev", Email = "nikolay@example.com", PhoneNumber = "+359888200103", JoinDate = ClubTime.Today.AddDays(-7) }
        };
        ClubTime.TryToUtc(ClubTime.Today.ToDateTime(new TimeOnly(18, 0)).AddDays(1), out var start);
        var sessions = new[]
        {
            new TrainingSession { Title = "Evening flow", Description = "A welcoming yoga session focused on balance, breathing, and mobility.", Trainer = trainers[0], StartDateTime = start, DurationMinutes = 60, Capacity = 12 },
            new TrainingSession { Title = "Build your strength", Description = "Guided full-body strength training. All experience levels are welcome.", Trainer = trainers[1], StartDateTime = start.AddDays(1), DurationMinutes = 75, Capacity = 8 },
            new TrainingSession { Title = "Cardio circuit", Description = "An energizing mix of cardio and bodyweight exercises.", Trainer = trainers[2], StartDateTime = start.AddDays(2), DurationMinutes = 45, Capacity = 2 }
        };
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.Trainers.AddRange(trainers);
        db.Members.AddRange(members);
        db.TrainingSessions.AddRange(sessions);
        db.TrainingEnrollments.AddRange(
            new TrainingEnrollment { Member = members[0], TrainingSession = sessions[0] },
            new TrainingEnrollment { Member = members[1], TrainingSession = sessions[0] },
            new TrainingEnrollment { Member = members[0], TrainingSession = sessions[2] },
            new TrainingEnrollment { Member = members[2], TrainingSession = sessions[2] });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

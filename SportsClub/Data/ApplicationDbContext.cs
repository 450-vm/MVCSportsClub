using Microsoft.EntityFrameworkCore;
using SportsClub.Models;

namespace SportsClub.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingEnrollment> TrainingEnrollments => Set<TrainingEnrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trainer>().Ignore(x => x.FullName);
        modelBuilder.Entity<Member>().Ignore(x => x.FullName);
        modelBuilder.Entity<TrainingSession>().Ignore(x => x.AvailablePlaces);
        modelBuilder.Entity<TrainingSession>()
            .Property(x => x.StartDateTime)
            .HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        modelBuilder.Entity<TrainingSession>().HasIndex(x => x.StartDateTime);
        modelBuilder.Entity<TrainingSession>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Session_Capacity", "Capacity BETWEEN 1 AND 1000");
            t.HasCheckConstraint("CK_Session_Duration", "DurationMinutes BETWEEN 1 AND 480");
            t.HasCheckConstraint("CK_Session_Title", "length(trim(Title)) BETWEEN 1 AND 100");
            t.HasCheckConstraint("CK_Session_Description", "length(trim(Description)) BETWEEN 1 AND 1000");
        });
        modelBuilder.Entity<TrainingSession>()
            .HasOne(x => x.Trainer).WithMany(x => x.TrainingSessions)
            .HasForeignKey(x => x.TrainerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TrainingEnrollment>().HasKey(x => new { x.MemberId, x.TrainingSessionId });
        modelBuilder.Entity<TrainingEnrollment>()
            .HasOne(x => x.Member).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrainingEnrollment>()
            .HasOne(x => x.TrainingSession).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
    }
}

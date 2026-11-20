using System.ComponentModel.DataAnnotations;

namespace SportsClub.Models;

public class TrainingSession
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Title { get; set; } = "";
    [Required, StringLength(1000)] public string Description { get; set; } = "";
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    // Persisted as a UTC DateTime; SQLite has no native timezone type.
    public DateTime StartDateTime { get; set; }
    [Range(1, 480)] public int DurationMinutes { get; set; }
    [Range(1, 1000)] public int Capacity { get; set; }
    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
    public int AvailablePlaces => Capacity - Enrollments.Count;
}

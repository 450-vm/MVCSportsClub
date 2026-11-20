using System.ComponentModel.DataAnnotations;

namespace SportsClub.Models;

public class Trainer
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string FirstName { get; set; } = "";
    [Required, StringLength(60)] public string LastName { get; set; } = "";
    [Required, StringLength(100)] public string Specialization { get; set; } = "";
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(30)] public string PhoneNumber { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}";
    public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
}

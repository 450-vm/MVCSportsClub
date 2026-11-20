using System.ComponentModel.DataAnnotations;

namespace SportsClub.Models;

public class Member
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string FirstName { get; set; } = "";
    [Required, StringLength(60)] public string LastName { get; set; } = "";
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(30)] public string PhoneNumber { get; set; } = "";
    public DateOnly JoinDate { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}

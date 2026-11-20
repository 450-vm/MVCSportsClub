using System.ComponentModel.DataAnnotations;
using SportsClub.Services;

namespace SportsClub.ViewModels;

public class PersonInput
{
    [Required, StringLength(60), Display(Name = "First name")]
    public string FirstName { get; set; } = "";
    [Required, StringLength(60), Display(Name = "Last name")]
    public string LastName { get; set; } = "";
    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = "";
    [Required, Phone, StringLength(30), Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = "";
}

public class TrainerInput : PersonInput
{
    [Required, StringLength(100)] public string Specialization { get; set; } = "";
}

public class MemberInput : PersonInput, IValidatableObject
{
    [Required, DataType(DataType.Date), Display(Name = "Join date")]
    public DateOnly? JoinDate { get; set; } = ClubTime.Today;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (JoinDate > ClubTime.Today)
            yield return new ValidationResult("Join date cannot be in the future.", [nameof(JoinDate)]);
    }
}

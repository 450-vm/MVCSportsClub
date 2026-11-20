using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportsClub.Services;

namespace SportsClub.ViewModels;

public class SessionInput
{
    [Required, StringLength(100)] public string Title { get; set; } = "";
    [Required, StringLength(1000)] public string Description { get; set; } = "";
    [Required, Range(1, int.MaxValue), Display(Name = "Trainer")]
    public int? TrainerId { get; set; }
    [Required, Display(Name = "Start date and time (Sofia)")]
    public DateTime? StartLocal { get; set; } = ClubTime.LocalNow.AddDays(1).Date.AddHours(18);
    [Range(1, 480), Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; } = 60;
    [Range(1, 1000)] public int Capacity { get; set; } = 12;
    [ValidateNever] public List<SelectListItem> Trainers { get; set; } = [];
}

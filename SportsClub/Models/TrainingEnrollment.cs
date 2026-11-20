namespace SportsClub.Models;

public class TrainingEnrollment
{
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int TrainingSessionId { get; set; }
    public TrainingSession TrainingSession { get; set; } = null!;
}

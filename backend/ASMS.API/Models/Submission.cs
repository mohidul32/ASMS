namespace ASMS.API.Models;

public class Submission
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public int StudentId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted"; // Submitted, Reviewed, Returned
    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Assignment Assignment { get; set; } = null!;
    public User Student { get; set; } = null!;
}

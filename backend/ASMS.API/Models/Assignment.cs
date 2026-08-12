namespace ASMS.API.Models;

public class Assignment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public int ClassId { get; set; }
    public int TeacherId { get; set; }
    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public bool IsPublished { get; set; } = false;
    public bool AllowLateUpdate { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Subject Subject { get; set; } = null!;
    public Class Class { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = [];
}

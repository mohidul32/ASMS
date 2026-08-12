namespace ASMS.API.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public int? TeacherId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Class Class { get; set; } = null!;
    public User? Teacher { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = [];
}

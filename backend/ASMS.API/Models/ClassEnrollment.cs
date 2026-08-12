namespace ASMS.API.Models;

public class ClassEnrollment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ClassId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Class Class { get; set; } = null!;
}

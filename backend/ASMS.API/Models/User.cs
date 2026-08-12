namespace ASMS.API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, Teacher, Student
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClassEnrollment> Enrollments { get; set; } = [];
    public ICollection<Assignment> Assignments { get; set; } = [];
    public ICollection<Submission> Submissions { get; set; } = [];
}

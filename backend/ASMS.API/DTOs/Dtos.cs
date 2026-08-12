namespace ASMS.API.DTOs;

// Auth
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Role, string Name, int Id);

// User
public record CreateUserRequest(string Name, string Email, string Password, string Role);
public record UpdateUserRequest(string Name, string Email, bool IsActive);
public record UserResponse(int Id, string Name, string Email, string Role, bool IsActive, DateTime CreatedAt);

// Class
public record CreateClassRequest(string Name, string? Description);
public record UpdateClassRequest(string Name, string? Description, bool IsActive);
public record ClassResponse(int Id, string Name, string? Description, bool IsActive);

// Subject
public record CreateSubjectRequest(string Name, int ClassId, int? TeacherId);
public record UpdateSubjectRequest(string Name, int? TeacherId);
public record SubjectResponse(int Id, string Name, int ClassId, string ClassName, int? TeacherId, string? TeacherName);

// Enrollment
public record EnrollRequest(int UserId, int ClassId);

// Assignment
public record CreateAssignmentRequest(string Title, string Description, int SubjectId, int ClassId, DateTime Deadline, int MaxMarks, bool IsPublished, bool AllowLateUpdate);
public record UpdateAssignmentRequest(string Title, string Description, DateTime Deadline, int MaxMarks, bool IsPublished, bool AllowLateUpdate);
public record AssignmentResponse(int Id, string Title, string Description, int SubjectId, string SubjectName, int ClassId, string ClassName, int TeacherId, string TeacherName, DateTime Deadline, int MaxMarks, bool IsPublished, bool AllowLateUpdate, DateTime CreatedAt);

// Submission
public record CreateSubmissionRequest(int AssignmentId, string Answer);
public record UpdateSubmissionRequest(string Answer);
public record GradeSubmissionRequest(int Marks, string? Feedback, string Status);
public record SubmissionResponse(int Id, int AssignmentId, string AssignmentTitle, int StudentId, string StudentName, string Answer, string Status, int? Marks, string? Feedback, DateTime SubmittedAt, DateTime? UpdatedAt);

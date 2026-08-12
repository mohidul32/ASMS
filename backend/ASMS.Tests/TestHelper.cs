using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.Models;

namespace ASMS.Tests;

public static class TestHelper
{
    public static AppDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    public static AppDbContext CreateDbWithSeed(string dbName)
    {
        var db = CreateDb(dbName);

        db.Users.AddRange(
            new User { Id = 1, Name = "Admin", Email = "admin@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Role = "Admin" },
            new User { Id = 2, Name = "Teacher", Email = "teacher@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"), Role = "Teacher" },
            new User { Id = 3, Name = "Student", Email = "student@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"), Role = "Student" }
        );
        db.Classes.Add(new Class { Id = 1, Name = "Class 10" });
        db.Subjects.Add(new Subject { Id = 1, Name = "Math", ClassId = 1, TeacherId = 2 });
        db.ClassEnrollments.Add(new ClassEnrollment { Id = 1, UserId = 3, ClassId = 1 });
        db.Assignments.Add(new Assignment
        {
            Id = 1, Title = "Test Assignment", Description = "Desc",
            SubjectId = 1, ClassId = 1, TeacherId = 2,
            Deadline = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100, IsPublished = true, AllowLateUpdate = false
        });
        db.SaveChanges();
        return db;
    }
}

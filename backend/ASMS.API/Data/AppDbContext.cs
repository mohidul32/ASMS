using Microsoft.EntityFrameworkCore;
using ASMS.API.Models;

namespace ASMS.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassEnrollment> ClassEnrollments => Set<ClassEnrollment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<ClassEnrollment>()
            .HasIndex(e => new { e.UserId, e.ClassId }).IsUnique();

        modelBuilder.Entity<Subject>()
            .HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Teacher)
            .WithMany(u => u.Assignments)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Student)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Admin User", Email = "admin@asms.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Role = "Admin" },
            new User { Id = 2, Name = "Teacher One", Email = "teacher@asms.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"), Role = "Teacher" },
            new User { Id = 3, Name = "Student One", Email = "student@asms.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"), Role = "Student" }
        );

        modelBuilder.Entity<Class>().HasData(
            new Class { Id = 1, Name = "Class 10", Description = "Grade 10" }
        );

        modelBuilder.Entity<Subject>().HasData(
            new Subject { Id = 1, Name = "Mathematics", ClassId = 1, TeacherId = 2 }
        );

        modelBuilder.Entity<ClassEnrollment>().HasData(
            new ClassEnrollment { Id = 1, UserId = 3, ClassId = 1 }
        );
    }
}

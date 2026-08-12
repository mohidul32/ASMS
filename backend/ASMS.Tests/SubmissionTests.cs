using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ASMS.API.Controllers;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Models;

namespace ASMS.Tests;

public class SubmissionTests
{
    private static SubmissionsController CreateController(int userId, string role, AppDbContext db)
    {
        var controller = new SubmissionsController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role)
                ], "test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task Submit_ValidAssignment_ReturnsCreated()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Submit_ValidAssignment_ReturnsCreated));
        var controller = CreateController(3, "Student", db);

        var result = await controller.Submit(new CreateSubmissionRequest(1, "My answer"));

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Submit_PastDeadline_ReturnsBadRequest()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Submit_PastDeadline_ReturnsBadRequest));
        var assignment = await db.Assignments.FindAsync(1);
        assignment!.Deadline = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();

        var controller = CreateController(3, "Student", db);
        var result = await controller.Submit(new CreateSubmissionRequest(1, "My answer"));

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Deadline", bad.Value!.ToString());
    }

    [Fact]
    public async Task Submit_Duplicate_ReturnsBadRequest()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Submit_Duplicate_ReturnsBadRequest));
        db.Submissions.Add(new Submission { Id = 1, AssignmentId = 1, StudentId = 3, Answer = "First" });
        await db.SaveChangesAsync();

        var controller = CreateController(3, "Student", db);
        var result = await controller.Submit(new CreateSubmissionRequest(1, "Second"));

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Already submitted", bad.Value!.ToString());
    }

    [Fact]
    public async Task Update_PastDeadlineNoLateUpdate_ReturnsBadRequest()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Update_PastDeadlineNoLateUpdate_ReturnsBadRequest));
        var assignment = await db.Assignments.FindAsync(1);
        assignment!.Deadline = DateTime.UtcNow.AddDays(-1);
        assignment.AllowLateUpdate = false;
        db.Submissions.Add(new Submission { Id = 1, AssignmentId = 1, StudentId = 3, Answer = "First" });
        await db.SaveChangesAsync();

        var controller = CreateController(3, "Student", db);
        var result = await controller.Update(1, new UpdateSubmissionRequest("Updated answer"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_PastDeadlineWithLateUpdate_ReturnsOk()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Update_PastDeadlineWithLateUpdate_ReturnsOk));
        var assignment = await db.Assignments.FindAsync(1);
        assignment!.Deadline = DateTime.UtcNow.AddDays(-1);
        assignment.AllowLateUpdate = true;
        db.Submissions.Add(new Submission { Id = 1, AssignmentId = 1, StudentId = 3, Answer = "First" });
        await db.SaveChangesAsync();

        var controller = CreateController(3, "Student", db);
        var result = await controller.Update(1, new UpdateSubmissionRequest("Updated answer"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Grade_MarksExceedMax_ReturnsBadRequest()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Grade_MarksExceedMax_ReturnsBadRequest));
        db.Submissions.Add(new Submission { Id = 1, AssignmentId = 1, StudentId = 3, Answer = "Answer" });
        await db.SaveChangesAsync();

        var controller = CreateController(2, "Teacher", db);
        var result = await controller.Grade(1, new GradeSubmissionRequest(150, "Good", "Reviewed"));

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Marks cannot exceed", bad.Value!.ToString());
    }

    [Fact]
    public async Task Grade_ValidMarks_ReturnsOk()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Grade_ValidMarks_ReturnsOk));
        db.Submissions.Add(new Submission { Id = 1, AssignmentId = 1, StudentId = 3, Answer = "Answer" });
        await db.SaveChangesAsync();

        var controller = CreateController(2, "Teacher", db);
        var result = await controller.Grade(1, new GradeSubmissionRequest(85, "Well done", "Reviewed"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_AnotherStudentsSubmission_ReturnsForbid()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Update_AnotherStudentsSubmission_ReturnsForbid));
        db.Submissions.Add(new Submission { Id = 1, AssignmentId = 1, StudentId = 3, Answer = "Answer" });
        await db.SaveChangesAsync();

        // Student with Id=99 tries to update student 3's submission
        var controller = CreateController(99, "Student", db);
        var result = await controller.Update(1, new UpdateSubmissionRequest("Hacked"));

        Assert.IsType<ForbidResult>(result);
    }
}

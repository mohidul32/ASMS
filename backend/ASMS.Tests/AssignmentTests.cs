using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ASMS.API.Controllers;
using ASMS.API.Data;
using ASMS.API.DTOs;

namespace ASMS.Tests;

public class AssignmentTests
{
    private static AssignmentsController CreateController(int userId, string role, AppDbContext db)
    {
        var controller = new AssignmentsController(db);
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
    public async Task Create_AsTeacher_ReturnsCreated()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Create_AsTeacher_ReturnsCreated));
        var controller = CreateController(2, "Teacher", db);

        var result = await controller.Create(new CreateAssignmentRequest(
            "New Assignment", "Description", 1, 1,
            DateTime.UtcNow.AddDays(5), 50, true, false));

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Update_AnotherTeachersAssignment_ReturnsForbid()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Update_AnotherTeachersAssignment_ReturnsForbid));
        // Teacher with Id=99 tries to update assignment owned by teacher Id=2
        var controller = CreateController(99, "Teacher", db);

        var result = await controller.Update(1, new UpdateAssignmentRequest(
            "Hacked", "Hacked", DateTime.UtcNow.AddDays(1), 100, true, false));

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAll_AsStudent_ReturnsOnlyPublished()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(GetAll_AsStudent_ReturnsOnlyPublished));
        var controller = CreateController(3, "Student", db);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<AssignmentResponse>>(ok.Value);
        Assert.All(list, a => Assert.True(a.IsPublished));
    }

    [Fact]
    public async Task GetById_UnpublishedAsStudent_ReturnsForbid()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(GetById_UnpublishedAsStudent_ReturnsForbid));
        var assignment = await db.Assignments.FindAsync(1);
        assignment!.IsPublished = false;
        await db.SaveChangesAsync();

        var controller = CreateController(3, "Student", db);
        var result = await controller.GetById(1);

        Assert.IsType<ForbidResult>(result);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Models;

namespace ASMS.API.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserRole => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = db.Assignments
            .Include(a => a.Subject).Include(a => a.Class).Include(a => a.Teacher)
            .AsQueryable();

        if (UserRole == "Teacher")
            query = query.Where(a => a.TeacherId == UserId);
        else if (UserRole == "Student")
        {
            var classIds = await db.ClassEnrollments.Where(e => e.UserId == UserId).Select(e => e.ClassId).ToListAsync();
            query = query.Where(a => a.IsPublished && classIds.Contains(a.ClassId));
        }

        var result = await query.Select(a => new AssignmentResponse(
            a.Id, a.Title, a.Description, a.SubjectId, a.Subject.Name,
            a.ClassId, a.Class.Name, a.TeacherId, a.Teacher.Name,
            a.Deadline, a.MaxMarks, a.IsPublished, a.AllowLateUpdate, a.CreatedAt
        )).ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await db.Assignments.Include(a => a.Subject).Include(a => a.Class).Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (a == null) return NotFound();

        if (UserRole == "Student" && !a.IsPublished) return Forbid();

        return Ok(new AssignmentResponse(a.Id, a.Title, a.Description, a.SubjectId, a.Subject.Name,
            a.ClassId, a.Class.Name, a.TeacherId, a.Teacher.Name,
            a.Deadline, a.MaxMarks, a.IsPublished, a.AllowLateUpdate, a.CreatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create(CreateAssignmentRequest req)
    {
        var assignment = new Assignment
        {
            Title = req.Title, Description = req.Description,
            SubjectId = req.SubjectId, ClassId = req.ClassId,
            TeacherId = UserId, Deadline = req.Deadline,
            MaxMarks = req.MaxMarks, IsPublished = req.IsPublished,
            AllowLateUpdate = req.AllowLateUpdate
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, new { id = assignment.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(int id, UpdateAssignmentRequest req)
    {
        var assignment = await db.Assignments.FindAsync(id);
        if (assignment == null) return NotFound();
        if (assignment.TeacherId != UserId) return Forbid();

        assignment.Title = req.Title;
        assignment.Description = req.Description;
        assignment.Deadline = req.Deadline;
        assignment.MaxMarks = req.MaxMarks;
        assignment.IsPublished = req.IsPublished;
        assignment.AllowLateUpdate = req.AllowLateUpdate;
        await db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await db.Assignments.FindAsync(id);
        if (assignment == null) return NotFound();
        if (UserRole == "Teacher" && assignment.TeacherId != UserId) return Forbid();
        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

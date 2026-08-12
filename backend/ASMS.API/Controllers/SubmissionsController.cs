using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Models;

namespace ASMS.API.Controllers;

[ApiController]
[Route("api/submissions")]
[Authorize]
public class SubmissionsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserRole => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = db.Submissions.Include(s => s.Assignment).Include(s => s.Student).AsQueryable();

        if (UserRole == "Student")
            query = query.Where(s => s.StudentId == UserId);
        else if (UserRole == "Teacher")
            query = query.Where(s => s.Assignment.TeacherId == UserId);

        var result = await query.Select(s => new SubmissionResponse(
            s.Id, s.AssignmentId, s.Assignment.Title, s.StudentId, s.Student.Name,
            s.Answer, s.Status, s.Marks, s.Feedback, s.SubmittedAt, s.UpdatedAt
        )).ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await db.Submissions.Include(s => s.Assignment).Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (s == null) return NotFound();
        if (UserRole == "Student" && s.StudentId != UserId) return Forbid();
        if (UserRole == "Teacher" && s.Assignment.TeacherId != UserId) return Forbid();

        return Ok(new SubmissionResponse(s.Id, s.AssignmentId, s.Assignment.Title, s.StudentId,
            s.Student.Name, s.Answer, s.Status, s.Marks, s.Feedback, s.SubmittedAt, s.UpdatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit(CreateSubmissionRequest req)
    {
        var assignment = await db.Assignments.FindAsync(req.AssignmentId);
        if (assignment == null || !assignment.IsPublished) return BadRequest(new { message = "Assignment not found or not published" });
        if (assignment.Deadline < DateTime.UtcNow) return BadRequest(new { message = "Deadline has passed" });

        if (await db.Submissions.AnyAsync(s => s.AssignmentId == req.AssignmentId && s.StudentId == UserId))
            return BadRequest(new { message = "Already submitted" });

        var submission = new Submission { AssignmentId = req.AssignmentId, StudentId = UserId, Answer = req.Answer };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = submission.Id }, new { id = submission.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Update(int id, UpdateSubmissionRequest req)
    {
        var submission = await db.Submissions.Include(s => s.Assignment).FirstOrDefaultAsync(s => s.Id == id);
        if (submission == null) return NotFound();
        if (submission.StudentId != UserId) return Forbid();

        var pastDeadline = submission.Assignment.Deadline < DateTime.UtcNow;
        if (pastDeadline && !submission.Assignment.AllowLateUpdate)
            return BadRequest(new { message = "Deadline has passed and late updates are not allowed" });

        submission.Answer = req.Answer;
        submission.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpPut("{id}/grade")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Grade(int id, GradeSubmissionRequest req)
    {
        var submission = await db.Submissions.Include(s => s.Assignment).FirstOrDefaultAsync(s => s.Id == id);
        if (submission == null) return NotFound();
        if (submission.Assignment.TeacherId != UserId) return Forbid();
        if (req.Marks > submission.Assignment.MaxMarks)
            return BadRequest(new { message = $"Marks cannot exceed {submission.Assignment.MaxMarks}" });

        submission.Marks = req.Marks;
        submission.Feedback = req.Feedback;
        submission.Status = req.Status;
        await db.SaveChangesAsync();
        return Ok(new { message = "Graded" });
    }
}

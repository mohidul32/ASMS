using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Models;

namespace ASMS.API.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize]
public class SubjectsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Subjects.Include(s => s.Class).Include(s => s.Teacher)
            .Select(s => new SubjectResponse(s.Id, s.Name, s.ClassId, s.Class.Name, s.TeacherId, s.Teacher != null ? s.Teacher.Name : null))
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await db.Subjects.Include(s => s.Class).Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);
        return s == null ? NotFound() : Ok(new SubjectResponse(s.Id, s.Name, s.ClassId, s.Class.Name, s.TeacherId, s.Teacher?.Name));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateSubjectRequest req)
    {
        var subject = new Subject { Name = req.Name, ClassId = req.ClassId, TeacherId = req.TeacherId };
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject.Id);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateSubjectRequest req)
    {
        var subject = await db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();
        subject.Name = req.Name;
        subject.TeacherId = req.TeacherId;
        await db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var subject = await db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();
        db.Subjects.Remove(subject);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

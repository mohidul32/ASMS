using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Models;

namespace ASMS.API.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize]
public class ClassesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Classes.Select(c => new ClassResponse(c.Id, c.Name, c.Description, c.IsActive)).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await db.Classes.FindAsync(id);
        return c == null ? NotFound() : Ok(new ClassResponse(c.Id, c.Name, c.Description, c.IsActive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateClassRequest req)
    {
        var cls = new Class { Name = req.Name, Description = req.Description };
        db.Classes.Add(cls);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cls.Id }, new ClassResponse(cls.Id, cls.Name, cls.Description, cls.IsActive));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateClassRequest req)
    {
        var cls = await db.Classes.FindAsync(id);
        if (cls == null) return NotFound();
        cls.Name = req.Name;
        cls.Description = req.Description;
        cls.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(new ClassResponse(cls.Id, cls.Name, cls.Description, cls.IsActive));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var cls = await db.Classes.FindAsync(id);
        if (cls == null) return NotFound();
        db.Classes.Remove(cls);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/enroll")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Enroll(int id, EnrollRequest req)
    {
        if (await db.ClassEnrollments.AnyAsync(e => e.UserId == req.UserId && e.ClassId == id))
            return BadRequest(new { message = "Already enrolled" });

        db.ClassEnrollments.Add(new ClassEnrollment { UserId = req.UserId, ClassId = id });
        await db.SaveChangesAsync();
        return Ok(new { message = "Enrolled successfully" });
    }
}

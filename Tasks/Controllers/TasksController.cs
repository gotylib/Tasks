using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Tasks.Data;

namespace Tasks.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly TasksDbContext _context;

    public TasksController(TasksDbContext context)
    {
        _context = context;
    }

    [HttpGet("GetTasks")]
    public ActionResult<IEnumerable<TaskItem>> GetTasks()
    {
        return Ok( _context.TaskItems.ToList());
    }

    [HttpPost("AddTask")]
    public async Task<ActionResult> AddTask([FromBody] TaskItem task)
    {
        task.TaskDateTime = task.TaskDateTime.ToUniversalTime();
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("UpdateTask")]
    public async Task<ActionResult> UpdateTask([FromBody] TaskItem updatedTask)
    {
        updatedTask.TaskDateTime = updatedTask.TaskDateTime.ToUniversalTime();
        if (updatedTask == null || updatedTask.Id == 0)
        {
            return BadRequest("Invalid task data.");
        }

        var existingTask = _context.TaskItems.Find(updatedTask.Id);
        if (existingTask == null)
        {
            return NotFound();
        }

        // Обновляем свойства сущности
        existingTask.Name = updatedTask.Name;
        existingTask.Description = updatedTask.Description;
        existingTask.TaskDateTime = updatedTask.TaskDateTime;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("DeleteTask/{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

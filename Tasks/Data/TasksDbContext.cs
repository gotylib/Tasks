using Microsoft.EntityFrameworkCore;

namespace Tasks.Data;

public sealed class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<User> Users { get; set; }
    
}
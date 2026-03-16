using Microsoft.EntityFrameworkCore;
using App.Web.Models;

namespace App.Web.Data;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem
            {
                Id = 1,
                Title = "Build a .NET 10 Todo App",
                Notes = "Use ASP.NET Core MVC with EF Core InMemory",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Priority = Priority.High
            },
            new TodoItem
            {
                Id = 2,
                Title = "Add unit tests",
                Notes = "Cover controller actions and model validation",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                Priority = Priority.High
            },
            new TodoItem
            {
                Id = 3,
                Title = "Style the application",
                Notes = "Make it attractive — not purple!",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Priority = Priority.Medium
            },
            new TodoItem
            {
                Id = 4,
                Title = "Review pull request",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Priority = Priority.Low
            }
        );
    }
}

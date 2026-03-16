using System.ComponentModel.DataAnnotations;

namespace App.Web.Models;

public class TodoItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Notes must be 1000 characters or less.")]
    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public Priority Priority { get; set; } = Priority.Medium;
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2
}

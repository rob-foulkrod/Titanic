using System.ComponentModel.DataAnnotations;
using App.Web.Models;

namespace App.Web.Tests;

public class TodoItemModelTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var ctx = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void TodoItem_ValidModel_PassesValidation()
    {
        var item = new TodoItem { Title = "Valid task", Priority = Priority.Medium };
        var results = ValidateModel(item);
        Assert.Empty(results);
    }

    [Fact]
    public void TodoItem_EmptyTitle_FailsValidation()
    {
        var item = new TodoItem { Title = "", Priority = Priority.Medium };
        var results = ValidateModel(item);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TodoItem.Title)));
    }

    [Fact]
    public void TodoItem_TitleTooLong_FailsValidation()
    {
        var item = new TodoItem { Title = new string('a', 201), Priority = Priority.Medium };
        var results = ValidateModel(item);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TodoItem.Title)));
    }

    [Fact]
    public void TodoItem_NotesTooLong_FailsValidation()
    {
        var item = new TodoItem { Title = "Valid", Notes = new string('n', 1001), Priority = Priority.Medium };
        var results = ValidateModel(item);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TodoItem.Notes)));
    }

    [Fact]
    public void TodoItem_NullNotes_PassesValidation()
    {
        var item = new TodoItem { Title = "Valid", Notes = null, Priority = Priority.Medium };
        var results = ValidateModel(item);
        Assert.Empty(results);
    }

    [Fact]
    public void TodoItem_DefaultsAreSet()
    {
        var item = new TodoItem();
        Assert.Equal(Priority.Medium, item.Priority);
        Assert.False(item.IsCompleted);
        Assert.Null(item.CompletedAt);
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    public void TodoItem_AllPriorities_AreValid(Priority priority)
    {
        var item = new TodoItem { Title = "Task", Priority = priority };
        var results = ValidateModel(item);
        Assert.Empty(results);
    }

    [Fact]
    public void TodoItem_MaxLengthTitle_PassesValidation()
    {
        var item = new TodoItem { Title = new string('a', 200), Priority = Priority.Low };
        var results = ValidateModel(item);
        Assert.Empty(results);
    }

    [Fact]
    public void TodoItem_MaxLengthNotes_PassesValidation()
    {
        var item = new TodoItem { Title = "Task", Notes = new string('n', 1000), Priority = Priority.Low };
        var results = ValidateModel(item);
        Assert.Empty(results);
    }
}

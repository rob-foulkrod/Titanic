using App.Web.Controllers;
using App.Web.Data;
using App.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace App.Web.Tests;

public class TodosControllerTests : IDisposable
{
    private readonly TodoContext _context;
    private readonly TodosController _controller;

    public TodosControllerTests()
    {
        var options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TodoContext(options);
        _controller = new TodosController(_context);

        // Set up TempData so controller actions that set TempData don't throw NullReferenceException
        _controller.TempData = new FakeTempData();

        SeedData();
    }

    private void SeedData()
    {
        _context.TodoItems.AddRange(
            new TodoItem { Id = 1, Title = "First task", IsCompleted = false, Priority = Priority.High, CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new TodoItem { Id = 2, Title = "Second task", IsCompleted = true, CompletedAt = DateTime.UtcNow, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new TodoItem { Id = 3, Title = "Third task", IsCompleted = false, Priority = Priority.Low, Notes = "Some notes", CreatedAt = DateTime.UtcNow.AddHours(-1) }
        );
        _context.SaveChanges();
    }

    // --- Index ---

    [Fact]
    public async Task Index_NoFilter_ReturnsAllItems()
    {
        var result = await _controller.Index(null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(view.Model);
        Assert.Equal(3, model.Count());
    }

    [Fact]
    public async Task Index_FilterActive_ReturnsOnlyActiveItems()
    {
        var result = await _controller.Index("active");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(view.Model);
        Assert.All(model, item => Assert.False(item.IsCompleted));
    }

    [Fact]
    public async Task Index_FilterCompleted_ReturnsOnlyCompletedItems()
    {
        var result = await _controller.Index("completed");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(view.Model);
        Assert.All(model, item => Assert.True(item.IsCompleted));
    }

    [Fact]
    public async Task Index_SetsViewBagCounts()
    {
        await _controller.Index(null);

        Assert.Equal(3, (int)_controller.ViewBag.TotalCount);
        Assert.Equal(2, (int)_controller.ViewBag.ActiveCount);
        Assert.Equal(1, (int)_controller.ViewBag.CompletedCount);
    }

    // --- Details ---

    [Fact]
    public async Task Details_ValidId_ReturnsViewWithItem()
    {
        var result = await _controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TodoItem>(view.Model);
        Assert.Equal("First task", model.Title);
    }

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        var result = await _controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_InvalidId_ReturnsNotFound()
    {
        var result = await _controller.Details(999);
        Assert.IsType<NotFoundResult>(result);
    }

    // --- Create GET ---

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var result = _controller.Create();
        Assert.IsType<ViewResult>(result);
    }

    // --- Create POST ---

    [Fact]
    public async Task Create_ValidItem_RedirectsToIndex()
    {
        var item = new TodoItem { Title = "New todo", Priority = Priority.Medium };
        var result = await _controller.Create(item);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodosController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Create_ValidItem_PersistsToDatabase()
    {
        var item = new TodoItem { Title = "Persisted task", Priority = Priority.High };
        await _controller.Create(item);

        var saved = await _context.TodoItems.FirstOrDefaultAsync(t => t.Title == "Persisted task");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Create_InvalidItem_ReturnsView()
    {
        _controller.ModelState.AddModelError("Title", "Required");
        var item = new TodoItem { Title = "" };

        var result = await _controller.Create(item);
        Assert.IsType<ViewResult>(result);
    }

    // --- Edit GET ---

    [Fact]
    public async Task Edit_ValidId_ReturnsViewWithItem()
    {
        var result = await _controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TodoItem>(view.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task Edit_NullId_ReturnsNotFound()
    {
        var result = await _controller.Edit(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_InvalidId_ReturnsNotFound()
    {
        var result = await _controller.Edit(999);
        Assert.IsType<NotFoundResult>(result);
    }

    // --- Edit POST ---

    [Fact]
    public async Task Edit_ValidItem_UpdatesAndRedirects()
    {
        var item = new TodoItem { Id = 1, Title = "Updated Title", Priority = Priority.Low, CreatedAt = DateTime.UtcNow };
        var result = await _controller.Edit(1, item);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodosController.Index), redirect.ActionName);

        var updated = await _context.TodoItems.FindAsync(1);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task Edit_MismatchedId_ReturnsNotFound()
    {
        var item = new TodoItem { Id = 2, Title = "Mismatch", Priority = Priority.Low, CreatedAt = DateTime.UtcNow };
        var result = await _controller.Edit(1, item);
        Assert.IsType<NotFoundResult>(result);
    }

    // --- ToggleComplete ---

    [Fact]
    public async Task ToggleComplete_ActiveItem_MarksAsCompleted()
    {
        await _controller.ToggleComplete(1);

        var item = await _context.TodoItems.FindAsync(1);
        Assert.True(item!.IsCompleted);
        Assert.NotNull(item.CompletedAt);
    }

    [Fact]
    public async Task ToggleComplete_CompletedItem_MarksAsActive()
    {
        await _controller.ToggleComplete(2);

        var item = await _context.TodoItems.FindAsync(2);
        Assert.False(item!.IsCompleted);
        Assert.Null(item.CompletedAt);
    }

    [Fact]
    public async Task ToggleComplete_InvalidId_ReturnsNotFound()
    {
        var result = await _controller.ToggleComplete(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ToggleComplete_ValidId_RedirectsToIndex()
    {
        var result = await _controller.ToggleComplete(1);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodosController.Index), redirect.ActionName);
    }

    // --- Delete GET ---

    [Fact]
    public async Task Delete_ValidId_ReturnsViewWithItem()
    {
        var result = await _controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TodoItem>(view.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task Delete_NullId_ReturnsNotFound()
    {
        var result = await _controller.Delete(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        var result = await _controller.Delete(999);
        Assert.IsType<NotFoundResult>(result);
    }

    // --- DeleteConfirmed ---

    [Fact]
    public async Task DeleteConfirmed_ValidId_RemovesItemAndRedirects()
    {
        var result = await _controller.DeleteConfirmed(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodosController.Index), redirect.ActionName);
        Assert.Null(await _context.TodoItems.FindAsync(1));
    }

    [Fact]
    public async Task DeleteConfirmed_InvalidId_StillRedirects()
    {
        var result = await _controller.DeleteConfirmed(999);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodosController.Index), redirect.ActionName);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

/// <summary>Minimal ITempDataDictionary stub for unit tests.</summary>
internal sealed class FakeTempData : Dictionary<string, object?>, Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary
{
    public void Keep() { }
    public void Keep(string key) { }
    public void Load() { }
    public void Save() { }
    public IEnumerable<string> PeekKeys() => Keys;
    public object? Peek(string key) => TryGetValue(key, out var v) ? v : null;
}


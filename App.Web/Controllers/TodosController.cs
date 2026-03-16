using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Web.Data;
using App.Web.Models;

namespace App.Web.Controllers;

public class TodosController : Controller
{
    private readonly TodoContext _context;

    public TodosController(TodoContext context)
    {
        _context = context;
    }

    // GET: Todos
    public async Task<IActionResult> Index(string? filter)
    {
        ViewBag.Filter = filter ?? "all";

        var todos = _context.TodoItems.AsQueryable();

        todos = filter switch
        {
            "active" => todos.Where(t => !t.IsCompleted),
            "completed" => todos.Where(t => t.IsCompleted),
            _ => todos
        };

        var items = await todos
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.IsCompleted)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.TotalCount = await _context.TodoItems.CountAsync();
        ViewBag.ActiveCount = await _context.TodoItems.CountAsync(t => !t.IsCompleted);
        ViewBag.CompletedCount = await _context.TodoItems.CountAsync(t => t.IsCompleted);

        return View(items);
    }

    // GET: Todos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(m => m.Id == id);
        if (todoItem == null)
            return NotFound();

        return View(todoItem);
    }

    // GET: Todos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Todos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Notes,Priority")] TodoItem todoItem)
    {
        if (ModelState.IsValid)
        {
            todoItem.CreatedAt = DateTime.UtcNow;
            _context.Add(todoItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"'{todoItem.Title}' was added to your list!";
            return RedirectToAction(nameof(Index));
        }
        return View(todoItem);
    }

    // GET: Todos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
            return NotFound();

        return View(todoItem);
    }

    // POST: Todos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Notes,IsCompleted,Priority,CreatedAt")] TodoItem todoItem)
    {
        if (id != todoItem.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await _context.TodoItems.FindAsync(id);
                if (existing == null)
                    return NotFound();

                if (todoItem.IsCompleted && !existing.IsCompleted)
                    todoItem.CompletedAt = DateTime.UtcNow;
                else if (!todoItem.IsCompleted)
                    todoItem.CompletedAt = null;
                else
                    todoItem.CompletedAt = existing.CompletedAt;

                _context.Entry(existing).CurrentValues.SetValues(todoItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{todoItem.Title}' was updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TodoItems.AnyAsync(e => e.Id == todoItem.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(todoItem);
    }

    // POST: Todos/ToggleComplete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
            return NotFound();

        todoItem.IsCompleted = !todoItem.IsCompleted;
        todoItem.CompletedAt = todoItem.IsCompleted ? DateTime.UtcNow : null;

        _context.Update(todoItem);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Todos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(m => m.Id == id);
        if (todoItem == null)
            return NotFound();

        return View(todoItem);
    }

    // POST: Todos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem != null)
        {
            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Task was deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}

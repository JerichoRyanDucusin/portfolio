using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers;

// The editor side: answers every address that starts with /ManageCourses.
public class ManageCoursesController : Controller
{
    private readonly CourseRepository _courses;

    // ASP.NET Core hands in the repository, because Program.cs registered it.
    public ManageCoursesController(CourseRepository courses)
    {
        _courses = courses;
    }

    // GET /ManageCourses : the list, with Edit and Delete buttons.
    public async Task<IActionResult> Index()
    {
        var courses = await _courses.GetAllAsync();
        return View(courses);
    }

    // GET /ManageCourses/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Course());
    }

    // POST /ManageCourses/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View(course);
        }

        try
        {
            await _courses.AddAsync(course);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Code", "Another course already uses this code.");
            return View(course);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /ManageCourses/Edit/5 : the form, filled in with the course.
    public async Task<IActionResult> Edit(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST /ManageCourses/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View(course);
        }

        try
        {
            await _courses.UpdateAsync(course);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Code", "Another course already uses this code.");
            return View(course);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /ManageCourses/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST /ManageCourses/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _courses.DeleteAsync(id);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.Error = "This course still has projects, so the database will not delete it. Delete or move its projects first.";
            return View("Delete", course);
        }

        return RedirectToAction(nameof(Index));
    }
}
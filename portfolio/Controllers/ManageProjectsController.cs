using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers;

// The editor side: answers every address that starts with /ManageProjects.
[Authorize]
public class ManageProjectsController : Controller
{
    // An image address: http(s), then letters, then an image extension (and maybe ?something).
    private const string ImagePattern = @"^https?://\S+\.(png|jpe?g|gif|webp|svg)(\?\S*)?$";

    private readonly ProjectRepository _projects;
    private readonly ProjectImageRepository _images;
    private readonly CourseRepository _courses;

    public ManageProjectsController(ProjectRepository projects, ProjectImageRepository images, CourseRepository courses)
    {
        _projects = projects;
        _images = images;
        _courses = courses;
    }

    // Fills the course dropdown of the form.
    private async Task FillCoursesAsync()
    {
        ViewBag.Courses = new SelectList(await _courses.GetAllAsync(), "CourseId", "Code");
    }

    // Splits the textbox into one address per line, without blanks.
    private static List<string> ParseUrls(string? text)
    {
        return (text ?? "")
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line != "")
            .ToList();
    }

    // Checks every image address, line by line, with Regex.IsMatch.
    private void CheckImageUrls(List<string> urls)
    {
        for (int i = 0; i < urls.Count; i++)
        {
            if (urls[i].Length > 300)
            {
                ModelState.AddModelError(nameof(ProjectForm.ImageUrls), $"Image {i + 1} is longer than 300 characters.");
            }
            else if (!Regex.IsMatch(urls[i], ImagePattern, RegexOptions.IgnoreCase))
            {
                ModelState.AddModelError(nameof(ProjectForm.ImageUrls),
                    $"Image {i + 1} is not a valid image address. Use a link that ends in .png, .jpg, .jpeg, .gif, .webp or .svg.");
            }
        }
    }

    // Turns a refusal from the database into a message on the page. Returns true when it did.
    private bool Explain(PostgresException ex)
    {
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation && ex.ConstraintName == "uq_image_per_project")
        {
            ModelState.AddModelError(nameof(ProjectForm.ImageUrls), "The same image address appears twice. Nothing was saved.");
            return true;
        }
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation && ex.ConstraintName == "uq_project_title")
        {
            ModelState.AddModelError(nameof(ProjectForm.Title), "This course already has a project with this title.");
            return true;
        }
        if (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            ModelState.AddModelError(nameof(ProjectForm.CourseId), "That course does not exist any more.");
            return true;
        }
        if (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            ModelState.AddModelError(nameof(ProjectForm.CompletedOn), "The completion date must be 2020-01-01 or later.");
            return true;
        }
        return false;
    }

    private static Project ToProject(ProjectForm form)
    {
        return new Project
        {
            ProjectId = form.ProjectId,
            CourseId = form.CourseId!.Value,
            Title = form.Title,
            Summary = form.Summary,
            VideoUrl = form.VideoUrl,
            CompletedOn = form.CompletedOn
        };
    }

    // GET /ManageProjects : every project, with Edit and Delete buttons.
    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetAllAsync();
        return View(projects);
    }

    // GET /ManageProjects/Create : an empty form.
    public async Task<IActionResult> Create()
    {
        await FillCoursesAsync();
        return View(new ProjectForm());
    }

    // POST /ManageProjects/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectForm form)
    {
        var urls = ParseUrls(form.ImageUrls);
        CheckImageUrls(urls);

        if (!ModelState.IsValid)
        {
            await FillCoursesAsync();
            return View(form);
        }

        try
        {
            await _projects.AddWithImagesAsync(ToProject(form), urls);
        }
        catch (PostgresException ex) when (Explain(ex))
        {
            await FillCoursesAsync();
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /ManageProjects/Edit/5 : the form, filled in with the project and its images.
    public async Task<IActionResult> Edit(long id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var images = await _images.GetByProjectAsync(id);
        var form = new ProjectForm
        {
            ProjectId = project.ProjectId,
            CourseId = project.CourseId,
            Title = project.Title,
            Summary = project.Summary,
            VideoUrl = project.VideoUrl,
            CompletedOn = project.CompletedOn,
            ImageUrls = string.Join(Environment.NewLine, images.Select(image => image.ImageUrl))
        };

        await FillCoursesAsync();
        return View(form);
    }

    // POST /ManageProjects/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectForm form)
    {
        var urls = ParseUrls(form.ImageUrls);
        CheckImageUrls(urls);

        if (!ModelState.IsValid)
        {
            await FillCoursesAsync();
            return View(form);
        }

        try
        {
            await _projects.UpdateWithImagesAsync(ToProject(form), urls);
        }
        catch (PostgresException ex) when (Explain(ex))
        {
            await FillCoursesAsync();
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /ManageProjects/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    // POST /ManageProjects/Delete/5 : the delete was confirmed. Its images go with it.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _projects.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
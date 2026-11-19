using Microsoft.AspNetCore.Mvc;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers;

// The viewer side: answers every address that starts with /Projects.
public class ProjectsController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly ProjectImageRepository _images;

    public ProjectsController(ProjectRepository projects, ProjectImageRepository images)
    {
        _projects = projects;
        _images = images;
    }

    // GET /Projects/Details/5 : one project, with its images and video.
    public async Task<IActionResult> Details(long id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var details = new ProjectDetails
        {
            Project = project,
            Images = await _images.GetByProjectAsync(id)
        };
        return View(details);
    }
}
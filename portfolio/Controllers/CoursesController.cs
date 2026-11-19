using Microsoft.AspNetCore.Mvc;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers;

// The viewer side: answers every address that starts with /Courses.
public class CoursesController : Controller
{
    private readonly CourseRepository _courses;
    private readonly ProjectRepository _projects;

    // ASP.NET Core hands in the repositories, because Program.cs registered them.
    public CoursesController(CourseRepository courses, ProjectRepository projects)
    {
        _courses = courses;
        _projects = projects;
    }

    // GET /Courses : every course.
    public async Task<IActionResult> Index()
    {
        var courses = await _courses.GetAllAsync();
        return View(courses);
    }

    // GET /Courses/Details/5 : one course and its projects.
    public async Task<IActionResult> Details(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        var details = new CourseDetails
        {
            Course = course,
            Projects = await _projects.GetByCourseAsync(id)
        };
        return View(details);
    }
}
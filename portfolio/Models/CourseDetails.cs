namespace portfolio.Models;

// The course page shows two things: the course, and its projects.
public class CourseDetails
{
    public Course Course { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}
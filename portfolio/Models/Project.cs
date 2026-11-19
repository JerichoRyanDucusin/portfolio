namespace portfolio.Models;

// One Project object holds one row of portfolio.project.
public class Project
{
    public long ProjectId { get; set; }           // project_id BIGINT, the primary key
    public long CourseId { get; set; }            // course_id BIGINT, the foreign key
    public string Title { get; set; } = "";       // title VARCHAR(100) NOT NULL
    public string? Summary { get; set; }          // summary VARCHAR(500), may be NULL
    public string? VideoUrl { get; set; }         // video_url VARCHAR(200), may be NULL
    public DateOnly CompletedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);   // completed_on DATE

    // Filled by the SELECT that joins course; never saved.
    public string CourseCode { get; set; } = "";
    public string CourseTitle { get; set; } = "";
}
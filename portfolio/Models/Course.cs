namespace portfolio.Models;

// One Course object holds one row of portfolio.course.
public class Course
{
    public long CourseId { get; set; }            // course_id BIGINT, the primary key
    public string Code { get; set; } = "";        // code VARCHAR(20) NOT NULL, UNIQUE
    public string Title { get; set; } = "";       // title VARCHAR(100) NOT NULL
    public string? Term { get; set; }             // term VARCHAR(30), may be NULL
    public string? Description { get; set; }      // description VARCHAR(500), may be NULL
}
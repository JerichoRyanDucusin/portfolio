using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

// What the project form sends: the project, plus its image links as one block of text.
public class ProjectForm
{
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "Choose a course.")]
    public long? CourseId { get; set; }

    [Required(ErrorMessage = "Enter a title.")]
    [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
    public string Title { get; set; } = "";

    [StringLength(500, ErrorMessage = "The summary can be at most 500 characters.")]
    public string? Summary { get; set; }

    [StringLength(200, ErrorMessage = "The video link can be at most 200 characters.")]
    [RegularExpression(@"^https?://(www\.)?(youtube\.com/(watch\?v=|shorts/)|youtu\.be/)[A-Za-z0-9_-]{11}\S*$",
        ErrorMessage = "Paste a YouTube link, such as https://www.youtube.com/watch?v=abcdefghijk.")]
    public string? VideoUrl { get; set; }

    public DateOnly CompletedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // One image address per line. Checked line by line in the controller.
    public string? ImageUrls { get; set; }
}
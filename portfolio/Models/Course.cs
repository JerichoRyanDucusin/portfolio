using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class Course
{
    public long CourseId { get; set; }

    [Required(ErrorMessage = "Enter a course code.")]
    [StringLength(20, ErrorMessage = "The code can be at most 20 characters.")]
    [RegularExpression(@"^[A-Z]{3,8}[0-9]{1,3}$",
        ErrorMessage = "Use capital letters then digits, such as MIXCPE1 or DBMWEB1.")]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Enter a title.")]
    [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
    public string Title { get; set; } = "";

    [StringLength(30, ErrorMessage = "The term can be at most 30 characters.")]
    public string? Term { get; set; }

    public string? Description { get; set; }
}
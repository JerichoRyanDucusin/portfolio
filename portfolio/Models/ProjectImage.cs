namespace portfolio.Models;

// One ProjectImage object holds one row of portfolio.project_image.
public class ProjectImage
{
    public long ImageId { get; set; }             // image_id BIGINT, the primary key
    public long ProjectId { get; set; }           // project_id BIGINT, the foreign key
    public string ImageUrl { get; set; } = "";    // image_url VARCHAR(300) NOT NULL
    public int Position { get; set; } = 1;        // position INTEGER, 1 shows first
}
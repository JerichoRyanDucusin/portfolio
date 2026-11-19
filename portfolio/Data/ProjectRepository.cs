using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

// Every SQL statement about projects lives in this one class.
public class ProjectRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row into a Project. Positions 0 to 7 match every project SELECT.
    private static Project ReadProject(NpgsqlDataReader reader)
    {
        return new Project
        {
            ProjectId = reader.GetInt64(0),
            CourseId = reader.GetInt64(1),
            Title = reader.GetString(2),
            Summary = reader.IsDBNull(3) ? null : reader.GetString(3),
            VideoUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
            CompletedOn = reader.GetFieldValue<DateOnly>(5),
            CourseCode = reader.GetString(6),
            CourseTitle = reader.GetString(7)
        };
    }

    private const string SelectProjects =
        "SELECT p.project_id, p.course_id, p.title, p.summary, p.video_url, p.completed_on, c.code, c.title " +
        "FROM portfolio.project p JOIN portfolio.course c ON c.course_id = p.course_id ";

    // READ: every project, newest first.
    public async Task<List<Project>> GetAllAsync()
    {
        const string sql = SelectProjects + "ORDER BY p.completed_on DESC, p.project_id DESC;";

        var projects = new List<Project>();

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(ReadProject(reader));
        }

        return projects;
    }

    // READ: the projects of one course, newest first.
    public async Task<List<Project>> GetByCourseAsync(long courseId)
    {
        const string sql = SelectProjects + "WHERE p.course_id = @course_id " +
                           "ORDER BY p.completed_on DESC, p.project_id DESC;";

        var projects = new List<Project>();

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("course_id", courseId);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(ReadProject(reader));
        }

        return projects;
    }

    // READ: one project by id, or null if there is none.
    public async Task<Project?> GetByIdAsync(long id)
    {
        const string sql = SelectProjects + "WHERE p.project_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadProject(reader) : null;
    }

    // UPDATE: change one project (its images are changed in ProjectImageRepository).
    public async Task UpdateAsync(Project project)
    {
        const string sql = "UPDATE portfolio.project " +
                           "SET course_id = @course_id, title = @title, summary = @summary, " +
                           "    video_url = @video_url, completed_on = @completed_on " +
                           "WHERE project_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("course_id", project.CourseId);
        command.Parameters.AddWithValue("title", project.Title.Trim());
        command.Parameters.AddWithValue("summary", Db.ToDb(project.Summary));
        command.Parameters.AddWithValue("video_url", Db.ToDb(project.VideoUrl));
        command.Parameters.AddWithValue("completed_on", project.CompletedOn);
        command.Parameters.AddWithValue("id", project.ProjectId);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one project. Its images go with it (ON DELETE CASCADE).
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM portfolio.project WHERE project_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }

    // (Adding a project together with its images, in one transaction, comes in a later step.)
}
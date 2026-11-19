using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

// Every SQL statement about project images lives in this one class.
public class ProjectImageRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectImageRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row into a ProjectImage. Positions 0 to 3 match every image SELECT.
    private static ProjectImage ReadImage(NpgsqlDataReader reader)
    {
        return new ProjectImage
        {
            ImageId = reader.GetInt64(0),
            ProjectId = reader.GetInt64(1),
            ImageUrl = reader.GetString(2),
            Position = reader.GetInt32(3)
        };
    }

    // READ: the images of one project, in their order.
    public async Task<List<ProjectImage>> GetByProjectAsync(long projectId)
    {
        const string sql = "SELECT image_id, project_id, image_url, position " +
                           "FROM portfolio.project_image WHERE project_id = @project_id " +
                           "ORDER BY position, image_id;";

        var images = new List<ProjectImage>();

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("project_id", projectId);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            images.Add(ReadImage(reader));
        }

        return images;
    }

    // READ: one image by id, or null if there is none.
    public async Task<ProjectImage?> GetByIdAsync(long id)
    {
        const string sql = "SELECT image_id, project_id, image_url, position " +
                           "FROM portfolio.project_image WHERE image_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadImage(reader) : null;
    }

    // CREATE: add one image to a project.
    public async Task AddAsync(ProjectImage image)
    {
        const string sql = "INSERT INTO portfolio.project_image (project_id, image_url, position) " +
                           "VALUES (@project_id, @image_url, @position);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("project_id", image.ProjectId);
        command.Parameters.AddWithValue("image_url", image.ImageUrl.Trim());
        command.Parameters.AddWithValue("position", image.Position);

        await command.ExecuteNonQueryAsync();
    }

    // UPDATE: change one image.
    public async Task UpdateAsync(ProjectImage image)
    {
        const string sql = "UPDATE portfolio.project_image " +
                           "SET image_url = @image_url, position = @position " +
                           "WHERE image_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("image_url", image.ImageUrl.Trim());
        command.Parameters.AddWithValue("position", image.Position);
        command.Parameters.AddWithValue("id", image.ImageId);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one image.
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM portfolio.project_image WHERE image_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
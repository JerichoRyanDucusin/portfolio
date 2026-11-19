using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

// Every SQL statement about courses lives in this one class.
public class CourseRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CourseRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row into a Course. Positions 0 to 4 match every course SELECT.
    private static Course ReadCourse(NpgsqlDataReader reader)
    {
        return new Course
        {
            CourseId = reader.GetInt64(0),
            Code = reader.GetString(1),
            Title = reader.GetString(2),
            Term = reader.IsDBNull(3) ? null : reader.GetString(3),
            Description = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }

    // READ: every course, sorted by code.
    public async Task<List<Course>> GetAllAsync()
    {
        const string sql = "SELECT course_id, code, title, term, description " +
                           "FROM portfolio.course ORDER BY code;";

        var courses = new List<Course>();

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            courses.Add(ReadCourse(reader));
        }

        return courses;
    }

    // READ: one course by id, or null if there is none.
    public async Task<Course?> GetByIdAsync(long id)
    {
        const string sql = "SELECT course_id, code, title, term, description " +
                           "FROM portfolio.course WHERE course_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadCourse(reader) : null;
    }

    // CREATE: insert a new course.
    public async Task AddAsync(Course course)
    {
        const string sql = "INSERT INTO portfolio.course (code, title, term, description) " +
                           "VALUES (@code, @title, @term, @description);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("code", course.Code.Trim());
        command.Parameters.AddWithValue("title", course.Title.Trim());
        command.Parameters.AddWithValue("term", Db.ToDb(course.Term));
        command.Parameters.AddWithValue("description", Db.ToDb(course.Description));

        await command.ExecuteNonQueryAsync();
    }

    // UPDATE: change one course.
    public async Task UpdateAsync(Course course)
    {
        const string sql = "UPDATE portfolio.course " +
                           "SET code = @code, title = @title, term = @term, description = @description " +
                           "WHERE course_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("code", course.Code.Trim());
        command.Parameters.AddWithValue("title", course.Title.Trim());
        command.Parameters.AddWithValue("term", Db.ToDb(course.Term));
        command.Parameters.AddWithValue("description", Db.ToDb(course.Description));
        command.Parameters.AddWithValue("id", course.CourseId);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one course. Fails if the course still has projects (ON DELETE RESTRICT).
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM portfolio.course WHERE course_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
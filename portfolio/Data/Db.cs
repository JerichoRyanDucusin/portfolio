namespace portfolio.Data;

// Small helpers shared by the repositories.
public static class Db
{
    // Blank text is stored as NULL, not as an empty string.
    public static object ToDb(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? DBNull.Value : text.Trim();
    }
}
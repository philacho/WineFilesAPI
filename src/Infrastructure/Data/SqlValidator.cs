using System.Text.RegularExpressions;

namespace WineFilesApi.Infrastructure.Data;

public static class SqlValidator
{
    // Words that would allow data modification or DDL.
    private static readonly HashSet<string> ForbiddenKeywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "INSERT", "UPDATE", "DELETE", "MERGE", "REPLACE",
            "DROP", "CREATE", "ALTER", "TRUNCATE", "RENAME",
            "GRANT", "REVOKE", "COMMIT", "ROLLBACK",
            "EXEC", "EXECUTE", "CALL",
            "INTO",   // blocks "SELECT ... INTO cursor"
            "ATTACH", "DETACH",
            "PRAGMA",
            "OPEN", "CLOSE", "USE", "SET"
        };

    // Block comment markers that could be used to hide attacks.
    private static readonly string[] ForbiddenTokens = ["--", "/*", "*/"];

    public static void EnsureReadOnlySelect(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL is required.");

        var trimmed = sql.Trim();

        // 1. Must start with SELECT
        if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only SELECT statements are allowed.");

        // 2. Must be a single statement — no trailing semicolon + more SQL
        //    Allow a single optional trailing semicolon.
        var withoutTrailingSemi = trimmed.TrimEnd(';').TrimEnd();
        if (withoutTrailingSemi.Contains(';'))
            throw new ArgumentException("Multiple statements are not allowed.");

        // 3. No forbidden keywords as whole words
        var upper = " " + withoutTrailingSemi.ToUpperInvariant() + " ";

        foreach (var kw in ForbiddenKeywords)
        {
            var pattern = $@"\b{Regex.Escape(kw)}\b";
            if (Regex.IsMatch(upper, pattern))
                throw new ArgumentException($"Keyword '{kw}' is not allowed.");
        }

        // 4. No comment markers
        foreach (var token in ForbiddenTokens)
        {
            if (withoutTrailingSemi.Contains(token, StringComparison.Ordinal))
                throw new ArgumentException("SQL comments are not allowed.");
        }
    }
}
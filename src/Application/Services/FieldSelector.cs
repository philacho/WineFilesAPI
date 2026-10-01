using System.Collections.Frozen;

namespace WineFilesApi.Application.Services;

/// <summary>
/// Parses a comma-separated "fields" query parameter and validates it
/// against a whitelist. Prevents SQL injection via the SELECT clause.
/// </summary>
public sealed class FieldSelector
{
    private readonly FrozenDictionary<string, string> _map; // public name -> column name

    public FieldSelector(IDictionary<string, string> publicToColumn)
    {
        // Case-insensitive lookup: client can send Description, DESCRIPTION, description
        _map = publicToColumn.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the list of column names to SELECT.
    /// If <paramref name="requested"/> is null/empty, returns every column.
    /// Throws <see cref="ArgumentException"/> for unknown field names.
    /// </summary>
    public IReadOnlyList<string> Resolve(string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested))
            return _map.Values.ToList();

        var requestedNames = requested
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (requestedNames.Length == 0)
            return _map.Values.ToList();

        var columns = new List<string>(requestedNames.Length);
        var invalid = new List<string>();

        foreach (var name in requestedNames)
        {
            if (_map.TryGetValue(name, out var column))
                columns.Add(column);
            else
                invalid.Add(name);
        }

        if (invalid.Count > 0)
            throw new ArgumentException(
                $"Unknown field(s): {string.Join(", ", invalid)}. " +
                $"Valid fields: {string.Join(", ", _map.Keys.OrderBy(k => k))}.");

        return columns;
    }

    /// <summary>All public field names, useful for API documentation.</summary>
    public IReadOnlyCollection<string> AllFields => _map.Keys;
}
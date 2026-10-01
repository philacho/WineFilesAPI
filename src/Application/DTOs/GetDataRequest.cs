namespace WineFilesApi.Application.DTOs;

public sealed class GetDataRequest
{
    public string Table { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public List<FoxProFilter> Filters { get; set; } = [];
}

public sealed class FoxProFilter
{
    public string Column { get; set; } = string.Empty;
    public string Operator { get; set; } = "=";
    public string? Value { get; set; }
}

public sealed class SqlQueryRequest
{
    /// <summary>The SELECT statement to execute.</summary>
    public string Sql { get; set; } = string.Empty;

    /// <summary>Optional cap on rows returned. Defaults to 1000, max 10000.</summary>
    public int? MaxRows { get; set; }
}
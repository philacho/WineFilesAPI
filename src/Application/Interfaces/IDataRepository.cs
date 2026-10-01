using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IDataRepository
{
    Task<IReadOnlyList<Dictionary<string, object?>>> GetDataAsync(
        string table,
        IReadOnlyList<string> columns,
        IReadOnlyList<FoxProFilter>? filters,
        CancellationToken ct);

    Task<IReadOnlyList<Dictionary<string, object?>>> ExecuteQueryAsync(
        string sql,
        int maxRows,
        CancellationToken ct);
}
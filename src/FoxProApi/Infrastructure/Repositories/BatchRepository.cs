using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class BatchRepository : IBatchRepository
{
    private readonly FoxProConnectionFactory _factory;

    public BatchRepository(FoxProConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<Batch>> GetAsync(string? search, CancellationToken ct)
    {
        const string baseSql = """
            SELECT FCBATCH, FCDESCRIPT, FCACTIVE, FCUSERLOCK
            FROM batch
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(
            string.IsNullOrWhiteSpace(search)
                ? baseSql + " ORDER BY FCBATCH"
                : baseSql + " WHERE FCBATCH LIKE ? OR FCDESCRIPT LIKE ? ORDER BY FCBATCH",
            cn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = $"%{search.Trim()}%";
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = value });
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = value });
        }

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Batch>();

        while (reader is not null && await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<Batch?> GetByIdAsync(string batch, CancellationToken ct)
    {
        const string sql = """
            SELECT FCBATCH, FCDESCRIPT, FCACTIVE, FCUSERLOCK
            FROM batch WHERE FCBATCH = ?
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = batch });

        using var reader = await cmd.ExecuteReaderAsync(ct);
        return reader is not null && await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<bool> InsertAsync(Batch x, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO batch (FCBATCH, FCDESCRIPT, FCACTIVE, FCUSERLOCK)
            VALUES (?, ?, ?, ?)
            """;
        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        Add(cmd, x);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(Batch x, CancellationToken ct)
    {
        const string sql = """
            UPDATE batch SET FCDESCRIPT = ?, FCACTIVE = ?, FCUSERLOCK = ?
            WHERE FCBATCH = ?
            """;
        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        Add(cmd, x.Description);
        Add(cmd, x.Active);
        Add(cmd, x.UserLock);
        Add(cmd, x.BatchCode);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(string batch, CancellationToken ct)
    {
        const string sql = "DELETE FROM batch WHERE FCBATCH = ?";
        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        Add(cmd, batch);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    // ✅ Changed: DbDataReader instead of OleDbDataReader
    private static Batch Map(DbDataReader r) => new()
    {
        BatchCode = S(r["FCBATCH"]),
        Description = S(r["FCDESCRIPT"]),
        Active = S(r["FCACTIVE"]),
        UserLock = S(r["FCUSERLOCK"])
    };

    private static string S(object value) =>
        value == DBNull.Value ? string.Empty : Convert.ToString(value) ?? string.Empty;

    private static void Add(OleDbCommand cmd, string value) =>
        cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = value ?? string.Empty });

    private static void Add(OleDbCommand cmd, Batch x)
    {
        Add(cmd, x.BatchCode);
        Add(cmd, x.Description);
        Add(cmd, x.Active);
        Add(cmd, x.UserLock);
    }
}
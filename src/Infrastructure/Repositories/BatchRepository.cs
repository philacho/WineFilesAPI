using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class BatchRepository(FoxProConnectionFactory factory) : IBatchRepository
{
    public async Task<IReadOnlyList<Batch>> GetAsync(
        string? search, IReadOnlyList<string> columns, CancellationToken ct)
    {
        var selectList = string.Join(", ", columns);
        var baseSql = $"SELECT {selectList} FROM batch";

        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(
            string.IsNullOrWhiteSpace(search)
                ? baseSql + " ORDER BY FCBATCH"
                : baseSql + " WHERE FCBATCH LIKE ? OR FCDESCRIPT LIKE ? ORDER BY FCBATCH",
            cn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var v = $"%{search.Trim()}%";
            cmd.Parameters.Add(OleDbParameters.VarChar(v));
            cmd.Parameters.Add(OleDbParameters.VarChar(v));
        }

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Batch>();
        while (await reader.ReadAsync(ct))
            list.Add(MapDynamic(reader, columns));
        return list;
    }

    public async Task<Batch?> GetByIdAsync(
        string batch, IReadOnlyList<string> columns, CancellationToken ct)
    {
        var selectList = string.Join(", ", columns);
        var sql = $"SELECT {selectList} FROM batch WHERE FCBATCH = ?";

        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(OleDbParameters.VarChar(batch));

        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapDynamic(reader, columns) : null;
    }

    public async Task<bool> InsertAsync(Batch x, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO batch (FCBATCH, FCDESCRIPT, FCACTIVE, FCUSERLOCK)
            VALUES (?, ?, ?, ?)
            """;

        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(OleDbParameters.VarChar(x.BatchCode));
        cmd.Parameters.Add(OleDbParameters.VarChar(x.Description));
        cmd.Parameters.Add(OleDbParameters.VarChar(x.Active));
        cmd.Parameters.Add(OleDbParameters.VarChar(x.UserLock));
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(Batch x, CancellationToken ct)
    {
        const string sql = """
            UPDATE batch SET FCDESCRIPT = ?, FCACTIVE = ?, FCUSERLOCK = ?
            WHERE FCBATCH = ?
            """;

        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(OleDbParameters.VarChar(x.Description));
        cmd.Parameters.Add(OleDbParameters.VarChar(x.Active));
        cmd.Parameters.Add(OleDbParameters.VarChar(x.UserLock));
        cmd.Parameters.Add(OleDbParameters.VarChar(x.BatchCode));
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(string batch, CancellationToken ct)
    {
        const string sql = "DELETE FROM batch WHERE FCBATCH = ?";
        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(OleDbParameters.VarChar(batch));
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    private static Batch MapDynamic(DbDataReader r, IReadOnlyList<string> columns)
    {
        var b = new Batch();
        foreach (var col in columns)
        {
            switch (col.ToUpperInvariant())
            {
                case "FCBATCH": b.BatchCode = S(r[col]); break;
                case "FCDESCRIPT": b.Description = S(r[col]); break;
                case "FCACTIVE": b.Active = S(r[col]); break;
                case "FCUSERLOCK": b.UserLock = S(r[col]); break;
            }
        }
        return b;
    }

    private static string S(object v) =>
        v == DBNull.Value ? string.Empty : Convert.ToString(v) ?? string.Empty;
}
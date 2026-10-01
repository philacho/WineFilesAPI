using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class PmIndexRepository : IPmIndexRepository
{
    private readonly FoxProConnectionFactory _factory;

    public PmIndexRepository(FoxProConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<PmIndex>> GetAsync(string? search, CancellationToken ct)
    {
        const string baseSql = """
            SELECT fccdxname, fctagname, fmtagkey, flunique, fldescend
            FROM pmindex
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(
            string.IsNullOrWhiteSpace(search)
                ? baseSql + " ORDER BY fccdxname, fctagname"
                : baseSql + " WHERE fccdxname LIKE ? OR fctagname LIKE ? ORDER BY fccdxname, fctagname",
            cn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var v = $"%{search.Trim()}%";
            cmd.Parameters.Add(VarChar(v));
            cmd.Parameters.Add(VarChar(v));
        }

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PmIndex>();
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    public async Task<IReadOnlyList<PmIndex>> GetByTableAsync(string table, CancellationToken ct)
    {
        // fccdxname in pmindex matches fctblname in pmdbf
        const string sql = """
            SELECT fccdxname, fctagname, fmtagkey, flunique, fldescend
            FROM pmindex
            WHERE fccdxname = ?
            ORDER BY fctagname
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(VarChar(table.Trim()));

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PmIndex>();
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    private static PmIndex Map(DbDataReader r) => new()
    {
        CdxName = S(r["fccdxname"]),
        TagName = S(r["fctagname"]),
        TagKey = S(r["fmtagkey"]),        // memo
        Unique = B(r["flunique"]),
        Descending = B(r["fldescend"])
    };

    private static string S(object v) =>
        v == DBNull.Value ? string.Empty : Convert.ToString(v) ?? string.Empty;

    // VFP logical fields sometimes come back as bool, sometimes as "T"/"F"/"Y"/"N"
    private static bool B(object v)
    {
        if (v == DBNull.Value) return false;
        if (v is bool b) return b;
        var s = Convert.ToString(v)?.Trim() ?? string.Empty;
        return s.Equals("T", StringComparison.OrdinalIgnoreCase)
            || s.Equals("Y", StringComparison.OrdinalIgnoreCase)
            || s.Equals("1", StringComparison.Ordinal);
    }

    private static OleDbParameter VarChar(string v) =>
        new() { OleDbType = OleDbType.VarChar, Value = v ?? string.Empty };
}
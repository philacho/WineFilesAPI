using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class PmDbfRepository : IPmDbfRepository
{
    private readonly FoxProConnectionFactory _factory;

    public PmDbfRepository(FoxProConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<PmDbf>> GetAsync(string? search, CancellationToken ct)
    {
        const string baseSql = """
            SELECT fctblname, fccdxname, fctype, fcpartbl, fclinkfld, fcdescript
            FROM pmdbf
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(
            string.IsNullOrWhiteSpace(search)
                ? baseSql + " ORDER BY fctblname"
                : baseSql + " WHERE fctblname LIKE ? OR fcdescript LIKE ? ORDER BY fctblname",
            cn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var v = $"%{search.Trim()}%";
            cmd.Parameters.Add(VarChar(v));
            cmd.Parameters.Add(VarChar(v));
        }

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PmDbf>();
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    public async Task<PmDbf?> GetByIdAsync(string tableName, CancellationToken ct)
    {
        const string sql = """
            SELECT fctblname, fccdxname, fctype, fcpartbl, fclinkfld, fcdescript
            FROM pmdbf WHERE fctblname = ?
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(VarChar(tableName));

        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private static PmDbf Map(DbDataReader r) => new()
    {
        TableName = S(r["fctblname"]),
        CdxName = S(r["fccdxname"]),
        TableType = S(r["fctype"]),
        ParentTable = S(r["fcpartbl"]),
        LinkField = S(r["fclinkfld"]),
        Description = S(r["fcdescript"])
    };

    private static string S(object v) =>
        v == DBNull.Value ? string.Empty : Convert.ToString(v) ?? string.Empty;

    private static OleDbParameter VarChar(string v) =>
        new() { OleDbType = OleDbType.VarChar, Value = v ?? string.Empty };
}
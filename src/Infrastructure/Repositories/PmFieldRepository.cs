using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class PmFieldRepository : IPmFieldRepository
{
    private readonly FoxProConnectionFactory _factory;

    public PmFieldRepository(FoxProConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<PmField>> GetAsync(string? table, CancellationToken ct)
    {
        const string baseSql = """
            SELECT fctblname, fcfldname, fcdatatype, fnfldlen, fnflddec,
                   fccolprmpt, fccoltitle
            FROM pmfield
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(
            string.IsNullOrWhiteSpace(table)
                ? baseSql + " ORDER BY fctblname, fcfldname"
                : baseSql + " WHERE fctblname = ? ORDER BY fctblname, fcfldname",
            cn);

        if (!string.IsNullOrWhiteSpace(table))
            cmd.Parameters.Add(VarChar(table.Trim()));

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PmField>();
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    public async Task<IReadOnlyList<PmField>> GetByTableAsync(string table, CancellationToken ct)
    {
        const string sql = """
            SELECT fctblname, fcfldname, fcdatatype, fnfldlen, fnflddec,
                   fccolprmpt, fccoltitle
            FROM pmfield
            WHERE fctblname = ?
            ORDER BY fcfldname
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        cmd.Parameters.Add(VarChar(table.Trim()));

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PmField>();
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    private static PmField Map(DbDataReader r) => new()
    {
        TableName = S(r["fctblname"]),
        FieldName = S(r["fcfldname"]),
        DataType = S(r["fcdatatype"]),
        FieldLen = I(r["fnfldlen"]),
        Decimals = I(r["fnflddec"]),
        ColPrompt = S(r["fccolprmpt"]),
        ColTitle = S(r["fccoltitle"])
    };

    private static string S(object v) =>
        v == DBNull.Value ? string.Empty : Convert.ToString(v) ?? string.Empty;

    private static int I(object v) =>
        v == DBNull.Value ? 0 : Convert.ToInt32(v);

    private static OleDbParameter VarChar(string v) =>
        new() { OleDbType = OleDbType.VarChar, Value = v ?? string.Empty };
}
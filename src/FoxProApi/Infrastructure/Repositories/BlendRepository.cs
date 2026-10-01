using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class BlendRepository : IBlendRepository
{
    private readonly FoxProConnectionFactory _factory;

    public BlendRepository(FoxProConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<Blend>> GetAsync(string? search, CancellationToken ct)
    {
        const string baseSql = """
            SELECT FCBLEND, FCACTIVE, FCCOMPATIB, FCDESCRIPT, FCGEOID,
                   FCGLCODE, FCNAMECODE, FCUSERLOCK, FCVARIETY, FNVINTAGE,
                   FCBLNDGRP, FCEXCEPTN, FMCOMMENTS
            FROM blend
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(
            string.IsNullOrWhiteSpace(search)
                ? baseSql + " ORDER BY FCBLEND"
                : baseSql + " WHERE FCBLEND LIKE ? OR FCDESCRIPT LIKE ? OR FCGEOID LIKE ? ORDER BY FCBLEND",
            cn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = $"%{search.Trim()}%";
            for (var i = 0; i < 3; i++)
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = value });
        }

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Blend>();

        while (reader is not null && await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<Blend?> GetByIdAsync(string blend, CancellationToken ct)
    {
        const string sql = """
            SELECT FCBLEND, FCACTIVE, FCCOMPATIB, FCDESCRIPT, FCGEOID,
                   FCGLCODE, FCNAMECODE, FCUSERLOCK, FCVARIETY, FNVINTAGE,
                   FCBLNDGRP, FCEXCEPTN, FMCOMMENTS
            FROM blend WHERE FCBLEND = ?
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        Add(cmd, blend);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        return reader is not null && await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<bool> InsertAsync(Blend x, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO blend
            (FCBLEND,FCACTIVE,FCCOMPATIB,FCDESCRIPT,FCGEOID,FCGLCODE,
             FCNAMECODE,FCUSERLOCK,FCVARIETY,FNVINTAGE,FCBLNDGRP,FCEXCEPTN,FMCOMMENTS)
            VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?)
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        AddAll(cmd, x);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(Blend x, CancellationToken ct)
    {
        const string sql = """
            UPDATE blend SET
                FCACTIVE=?, FCCOMPATIB=?, FCDESCRIPT=?, FCGEOID=?, FCGLCODE=?,
                FCNAMECODE=?, FCUSERLOCK=?, FCVARIETY=?, FNVINTAGE=?,
                FCBLNDGRP=?, FCEXCEPTN=?, FMCOMMENTS=?
            WHERE FCBLEND=?
            """;

        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);

        Add(cmd, x.Active);
        Add(cmd, x.Compatibility);
        Add(cmd, x.Description);
        Add(cmd, x.GeoId);
        Add(cmd, x.GlCode);
        Add(cmd, x.NameCode);
        Add(cmd, x.UserLock);
        Add(cmd, x.Variety);
        cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = x.Vintage });
        Add(cmd, x.BlendGroup);
        Add(cmd, x.ExceptionFlag);
        AddMemo(cmd, x.Comments);
        Add(cmd, x.BlendCode);

        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(string blend, CancellationToken ct)
    {
        const string sql = "DELETE FROM blend WHERE FCBLEND = ?";
        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);
        using var cmd = new OleDbCommand(sql, cn);
        Add(cmd, blend);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    // ✅ Changed: DbDataReader instead of OleDbDataReader
    private static Blend Map(DbDataReader r) => new()
    {
        BlendCode = S(r["FCBLEND"]),
        Active = S(r["FCACTIVE"]),
        Compatibility = S(r["FCCOMPATIB"]),
        Description = S(r["FCDESCRIPT"]),
        GeoId = S(r["FCGEOID"]),
        GlCode = S(r["FCGLCODE"]),
        NameCode = S(r["FCNAMECODE"]),
        UserLock = S(r["FCUSERLOCK"]),
        Variety = S(r["FCVARIETY"]),
        Vintage = I(r["FNVINTAGE"]),
        BlendGroup = S(r["FCBLNDGRP"]),
        ExceptionFlag = S(r["FCEXCEPTN"]),
        Comments = S(r["FMCOMMENTS"])
    };

    private static string S(object value) =>
        value == DBNull.Value ? string.Empty : Convert.ToString(value) ?? string.Empty;

    private static int I(object value) =>
        value == DBNull.Value ? 0 : Convert.ToInt32(value);

    private static void Add(OleDbCommand cmd, string value) =>
        cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = value ?? string.Empty });

    private static void AddMemo(OleDbCommand cmd, string value) =>
        cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.LongVarWChar, Value = value ?? string.Empty });

    private static void AddAll(OleDbCommand cmd, Blend x)
    {
        Add(cmd, x.BlendCode);
        Add(cmd, x.Active);
        Add(cmd, x.Compatibility);
        Add(cmd, x.Description);
        Add(cmd, x.GeoId);
        Add(cmd, x.GlCode);
        Add(cmd, x.NameCode);
        Add(cmd, x.UserLock);
        Add(cmd, x.Variety);
        cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = x.Vintage });
        Add(cmd, x.BlendGroup);
        Add(cmd, x.ExceptionFlag);
        AddMemo(cmd, x.Comments);
    }
}
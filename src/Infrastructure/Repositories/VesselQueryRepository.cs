using System.Data.OleDb;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class VesselQueryRepository(FoxProConnectionFactory factory) : IVesselQueryRepository
{
    public async Task<IReadOnlyList<VesselCompositionRowDto>> GetVesselCompositionAsync(
        string vessel, int lastOp, bool fromCn, string appLevel, CancellationToken ct)
    {
        // FoxPro: WHERE a.fcvessel = vcvessel AND a.fcblock = b.fcblock
        //         AND (if fromCn) a.fnopnumber = vnlastop
        var where = "a.fcvessel = ? AND a.fcblock = b.fcblock";
        var parameters = new List<OleDbParameter>
        {
            OleDbParameters.VarChar(vessel)
        };

        if (fromCn)
        {
            where += " AND a.fnopnumber = ?";
            parameters.Add(OleDbParameters.Integer(lastOp));
        }

        // FoxPro has two SELECT variants for appLevel 0 vs others,
        // but they return the same columns in different order — one SQL is fine.
        var sql = $"""
            SELECT a.fnvintage, a.fcvariety, a.fcblock, b.fcgeoid,
                   a.fnpercent AS fntemp
            FROM compostn a, block b
            WHERE {where}
            """;

        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(sql, cn);
        foreach (var p in parameters) cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<VesselCompositionRowDto>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new VesselCompositionRowDto
            {
                Vintage = I(reader["fnvintage"]),
                Variety = S(reader["fcvariety"]),
                Block = S(reader["fcblock"]),
                GeoId = S(reader["fcgeoid"]),
                // FoxPro: ROUND(fntemp * 100, 2)
                Percent = Math.Round(D(reader["fntemp"]) * 100m, 2),
                Description = string.Empty,
                Legend = string.Empty
            });
        }

        return list;
    }

    private static string S(object v) =>
        v == DBNull.Value ? string.Empty : Convert.ToString(v) ?? string.Empty;

    private static int I(object v) =>
        v == DBNull.Value ? 0 : Convert.ToInt32(v);

    private static decimal D(object v) =>
        v == DBNull.Value ? 0m : Convert.ToDecimal(v);
}
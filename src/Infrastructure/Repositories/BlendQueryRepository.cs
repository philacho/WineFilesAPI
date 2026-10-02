using System.Data.Common;
using System.Data.OleDb;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class BlendQueryRepository(FoxProConnectionFactory factory) : IBlendQueryRepository
{
    // -------------------- GetBlendVessels --------------------
    public async Task<IReadOnlyList<BlendVesselDto>> GetBlendVesselsAsync(
        string blend, string? batch, bool complete, CancellationToken ct)
    {
        // Build WHERE from FoxPro logic
        var lastOpField = complete ? "vessel.fnlastcmop" : "vessel.fnlastop";
        var where = $"{lastOpField} = opmaster.fnopnumber " +
                    "AND vessel.fcvessel == opmaster.fcvessel " +
                    "AND opmaster.fnvolume > 0 " +
                    "AND opmaster.fcblend = ?";

        var parameters = new List<OleDbParameter>
        {
            OleDbParameters.VarChar(blend)
        };

        // Batch logic
        if (!string.IsNullOrWhiteSpace(batch))
        {
            if (batch == "--------")
            {
                where += " AND EMPTY(opmaster.fcbatch)";
            }
            else
            {
                where += " AND opmaster.fcbatch = ?";
                parameters.Add(OleDbParameters.VarChar(batch));
            }
        }

        /*
        var sql = $"""
            SELECT vessel.fcvessel, opmaster.fcbatch, opmaster.fcstatus,
                   opmaster.fnvolume, vessel.fnlastcmop, vessel.fnlastop,
                   opmaster.fcmulti
            FROM vessel, opmaster
            WHERE {where}
            """;
        */

        var sql = $"""
            SELECT vessel.fcvessel, opmaster.fcbatch, opmaster.fcstatus,
                   opmaster.fnvolume, vessel.fnlastcmop, vessel.fnlastop,
                   '' as fcmulti
            FROM vessel, opmaster
            WHERE {where}
            """;

        return await ExecuteAsync(sql, parameters,
            r => new BlendVesselDto
            {
                Vessel = S(r["fcvessel"]),
                Batch = S(r["fcbatch"]),
                Status = S(r["fcstatus"]),
                Volume = I(r["fnvolume"]),
                LastCompletedOp = I(r["fnlastcmop"]),
                LastOp = I(r["fnlastop"]),
                Multi = S(r["fcmulti"])
            }, ct);
    }

    // -------------------- GetBlendComposition --------------------
    public async Task<IReadOnlyList<BlendCompositionRowDto>> GetBlendCompositionAsync(
        string blend, string? batch, string appLevel, CancellationToken ct)
    {
        var where =
            "vessel.fnlastcmop = opmaster.fnopnumber " +
            "AND vessel.fcvessel == opmaster.fcvessel " +
            "AND opmaster.fcblend = ? AND opmaster.fnvolume > 0 " +
            "AND compostn.fcvessel = vessel.fcvessel " +
            "AND compostn.fcblock = block.fcblock";

        var parameters = new List<OleDbParameter>
        {
            OleDbParameters.VarChar(blend)
        };

        if (!string.IsNullOrWhiteSpace(batch))
        {
            if (batch == "--------")
            {
                where += " AND EMPTY(opmaster.fcbatch)";
            }
            else
            {
                where += " AND opmaster.fcbatch = ?";
                parameters.Add(OleDbParameters.VarChar(batch));
            }
        }

        // FoxPro chose a different column order for appLevel 0 vs others
        // but the same underlying data — one SQL handles both.
        var sql = $"""
            SELECT compostn.fnvintage, compostn.fcvariety, compostn.fcblock,
                   block.fcgeoid,
                   SUM(compostn.fnpercent * opmaster.fnvolume) AS fnlitres
            FROM compostn, vessel, opmaster, block
            WHERE {where}
            GROUP BY compostn.fnvintage, compostn.fcvariety, compostn.fcblock,
                     block.fcgeoid
            """;

        return await ExecuteAsync(sql, parameters,
            r => new BlendCompositionRowDto
            {
                Vintage = I(r["fnvintage"]),
                Variety = S(r["fcvariety"]),
                Block = S(r["fcblock"]),
                GeoId = S(r["fcgeoid"]),
                Litres = D(r["fnlitres"]),
                Percent = 0m,      // computed in service
                Description = string.Empty,
                Legend = string.Empty
            }, ct);
    }

    // -------------------- GetBlendAdditives --------------------
    public async Task<IReadOnlyList<BlendAdditiveRowDto>> GetBlendAdditivesAsync(
        string blend, string? batch, string appLevel, CancellationToken ct)
    {
        var where =
            "vessel.fnlastcmop = opmaster.fnopnumber " +
            "AND vessel.fcvessel == opmaster.fcvessel " +
            "AND opmaster.fcblend = ? AND opmaster.fnvolume > 0 " +
            "AND vessaddv.fcvessel = vessel.fcvessel " +
            "AND vessaddv.fcopcode == opcode.fcopcode";

        var parameters = new List<OleDbParameter>
        {
            OleDbParameters.VarChar(blend)
        };

        if (!string.IsNullOrWhiteSpace(batch))
        {
            if (batch == "--------")
            {
                where += " AND EMPTY(opmaster.fcbatch)";
            }
            else
            {
                where += " AND opmaster.fcbatch = ?";
                parameters.Add(OleDbParameters.VarChar(batch));
            }
        }

        var sql = $"""
            SELECT vessaddv.fcopcode, opcode.fcdescript, vessaddv.fnamount,
                   vessaddv.fcunit, opmaster.fnvolume
            FROM vessaddv, vessel, opmaster, opcode
            WHERE {where}
            """;

        return await ExecuteAsync(sql, parameters,
            r => new BlendAdditiveRowDto
            {
                OpCode = S(r["fcopcode"]),
                Description = S(r["fcdescript"]),
                Amount = D(r["fnamount"]),
                Unit = S(r["fcunit"]),
                Volume = I(r["fnvolume"]),
                Result = string.Empty,
                AmountDisplay = string.Empty
            }, ct);
    }

    // -------------------- Shared executor --------------------
    private async Task<IReadOnlyList<T>> ExecuteAsync<T>(
        string sql, List<OleDbParameter> parameters,
        Func<DbDataReader, T> mapper, CancellationToken ct)
    {
        await using var cn = factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(sql, cn);
        foreach (var p in parameters) cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<T>();
        while (await reader.ReadAsync(ct))
            list.Add(mapper(reader));
        return list;
    }

    private static string S(object v) =>
        v == DBNull.Value ? string.Empty : Convert.ToString(v) ?? string.Empty;

    private static int I(object v) =>
        v == DBNull.Value ? 0 : Convert.ToInt32(v);

    private static decimal D(object v) =>
        v == DBNull.Value ? 0m : Convert.ToDecimal(v);
}
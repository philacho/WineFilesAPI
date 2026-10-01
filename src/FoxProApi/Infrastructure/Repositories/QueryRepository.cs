using System.Data.Common;
using System.Data.OleDb;
using System.Text;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Infrastructure.Data;

namespace WineFilesApi.Infrastructure.Repositories;

public sealed class QueryRepository : IQueryRepository
{
    private readonly FoxProConnectionFactory _factory;

    public QueryRepository(FoxProConnectionFactory factory) => _factory = factory;

    // ---------------------------------------------------------------
    // Query 1: blend / opmaster / vessel  ->  volume summary
    // ---------------------------------------------------------------
    public async Task<IReadOnlyList<IDictionary<string, object?>>> GetBlendBatchSummaryAsync(
        string? blend, string? batch, decimal? minVolume, int limit, CancellationToken ct)
    {
        // NOTE: FoxPro/OLE DB does not support named parameters reliably,
        // so we build the WHERE clause with '?' placeholders and add them
        // in the order they appear.
        var where = new StringBuilder("""
            blend.fcblend == opmaster.fcblend
            AND vessel.fcvessel == opmaster.fcvessel
            AND opmaster.fnvolume > 0
            AND vessel.fnlastop = opmaster.fnopnumber
            """);

        var parameters = new List<OleDbParameter>();

        if (!string.IsNullOrWhiteSpace(blend))
        {
            where.Append(" AND blend.fcblend = ?");
            parameters.Add(VarChar(blend.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(batch))
        {
            where.Append(" AND opmaster.fcbatch = ?");
            parameters.Add(VarChar(batch.Trim()));
        }

        if (minVolume.HasValue)
        {
            where.Append(" AND opmaster.fnvolume >= ?");
            parameters.Add(Numeric(minVolume.Value));
        }

        var sql = $"""
            SELECT blend.fcblend, opmaster.fcbatch, blend.fcdescript,
                   blend.fcvariety,
                   SUM(opmaster.fnvolume) AS fntotvol,
                   COUNT(*)               AS fnnumvsls,
                   0                      AS fntotcost,
                   0.0                    AS fncostvol,
                   []                     AS bbmulti,
                   000000                 AS vesvolume
            FROM vessel, opmaster, blend
            WHERE {where}
            GROUP BY blend.fcblend, opmaster.fcbatch, blend.fcdescript, blend.fcvariety
            """;

        return await ExecuteDynamicAsync(sql, parameters, limit, ct);
    }

    // ---------------------------------------------------------------
    // Query 2: vessel / opmaster / opheader  ->  operation detail
    // ---------------------------------------------------------------
    public async Task<IReadOnlyList<IDictionary<string, object?>>> GetVesselOpMasterAsync(
        decimal? capacityLow, decimal? capacityHigh, string? vessel,
        string? blend, int limit, CancellationToken ct)
    {
        var where = new StringBuilder("""
            opheader.fnopnumber = opmaster.fnopnumber
            AND vessel.fcvessel == opmaster.fcvessel
            AND vessel.fnlastcmop = opmaster.fnopnumber
            """);

        var parameters = new List<OleDbParameter>();

        if (capacityLow.HasValue && capacityHigh.HasValue)
        {
            where.Append(" AND vessel.fncapacity BETWEEN ? AND ?");
            parameters.Add(Numeric(capacityLow.Value));
            parameters.Add(Numeric(capacityHigh.Value));
        }
        else if (capacityLow.HasValue)
        {
            where.Append(" AND vessel.fncapacity >= ?");
            parameters.Add(Numeric(capacityLow.Value));
        }
        else if (capacityHigh.HasValue)
        {
            where.Append(" AND vessel.fncapacity <= ?");
            parameters.Add(Numeric(capacityHigh.Value));
        }

        where.Append(" AND opmaster.fnvolume >= vessel.fncapacity");

        if (!string.IsNullOrWhiteSpace(vessel))
        {
            where.Append(" AND vessel.fcvessel = ?");
            parameters.Add(VarChar(vessel.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(blend))
        {
            where.Append(" AND opmaster.fcblend = ?");
            parameters.Add(VarChar(blend.Trim()));
        }

        var sql = $"""
            SELECT vessel.fcvessel, opmaster.fcblend, opmaster.fcbatch,
                   opmaster.fcstatus, vessel.fcbrlgroup, vessel.fncapacity,
                   opmaster.fnvolume,
                   0000000 AS fnullage,
                   vessel.fnlastcmop AS fnopnumber,
                   opheader.fcopcode, opheader.fddate, opheader.fdedate,
                   vessel.fcvsltype, vessel.fclocation,
                   'Y' AS fccomplete, vessel.fcactive,
                   IIF(vessel.fnlastop = vessel.fnlastcmop, 'N', 'Y') AS fcopact,
                   opmaster.fccleanflg,
                   000000 AS dip,
                   vessel.fcnamecode, vessel.fdfirstuse, vessel.fdonsite,
                   vessel.fdoffsite,
                   000 AS age, vessel.flcontrol, vessel.fcbarc,
                   [ ] AS bown, [ ] AS bcomp, SPACE(40) AS bdesc,
                   vessel.fcattrib1, vessel.fcattrib2, vessel.fcattrib3,
                   vessel.fcattrib4, vessel.fcattrib5, vessel.fcattrib6,
                   [N] AS fcexceptn,
                   vessel.fcstack, vessel.fcrow, vessel.fcheight,
                   [      ] AS fcvariety,
                   0000 AS fnvintage,
                   [   ] AS fcvslclass,
                   [] AS bbmulti,
                   000000 AS vesvolume
            FROM vessel, opmaster, opheader
            WHERE {where}
            """;

        return await ExecuteDynamicAsync(sql, parameters, limit, ct);
    }

    // ---------------------------------------------------------------
    // Shared executor: returns column-name -> value dictionaries
    // ---------------------------------------------------------------
    private async Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteDynamicAsync(
        string sql, IEnumerable<OleDbParameter> parameters, int limit, CancellationToken ct)
    {
        await using var cn = _factory.CreateConnection();
        await cn.OpenAsync(ct);

        using var cmd = new OleDbCommand(sql, cn);
        foreach (var p in parameters)
            cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var rows = new List<IDictionary<string, object?>>();

        while (await reader.ReadAsync(ct) && rows.Count < limit)
        {
            var row = new Dictionary<string, object?>(
                reader.FieldCount, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[reader.GetName(i)] = value;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static OleDbParameter VarChar(string value) =>
        new() { OleDbType = OleDbType.VarChar, Value = value };

    private static OleDbParameter Numeric(decimal value) =>
        new() { OleDbType = OleDbType.Numeric, Value = value };
}
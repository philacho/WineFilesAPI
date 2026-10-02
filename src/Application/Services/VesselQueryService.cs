using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Application.Services;

public sealed class VesselQueryService(IVesselQueryRepository repo) : IVesselQueryService
{
    public async Task<IReadOnlyList<VesselCompositionRowDto>> GetVesselCompositionAsync(
        string vessel, int lastOp, bool fromCn, string appLevel, CancellationToken ct)
    {
        var rows = await repo.GetVesselCompositionAsync(vessel, lastOp, fromCn, appLevel, ct);
        if (rows.Count == 0) return rows;

        // Replace fntemp with fnpercent*100, round to 2 decimals
        foreach (var row in rows)
            row.Percent = Math.Round(row.Percent, 2);

        // Compute legends
        foreach (var row in rows)
        {
            var blockPart = appLevel == "0" ? row.Block : row.GeoId;
            row.Legend = $"{blockPart.Trim()} - {row.Variety.Trim()} - {row.Vintage,4}";
        }

        return rows;
    }
}
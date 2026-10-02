using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Application.Services;

public sealed class BlendQueryService(IBlendQueryRepository repo) : IBlendQueryService
{
    public Task<IReadOnlyList<BlendVesselDto>> GetBlendVesselsAsync(
        string blend, string? batch, bool complete, CancellationToken ct)
        => repo.GetBlendVesselsAsync(blend, batch, complete, ct);

    public async Task<IReadOnlyList<BlendCompositionRowDto>> GetBlendCompositionAsync(
        string blend, string? batch, string appLevel, CancellationToken ct)
    {
        var rows = await repo.GetBlendCompositionAsync(blend, batch, appLevel, ct);
        return ApplyCompositionEnrichment(rows);
    }

    public async Task<IReadOnlyList<BlendAdditiveRowDto>> GetBlendAdditivesAsync(
        string blend, string? batch, string appLevel, CancellationToken ct)
    {
        var rows = await repo.GetBlendAdditivesAsync(blend, batch, appLevel, ct);

        // Port of the additive post-processing from GetBlendAdditives.prg
        var processed = rows.Select(r =>
        {
            // 1. Convert to smallest unit
            var (smallAmount, smallUnit) = UnitConverter.ToSmallest(r.Amount, r.Unit);

            // 2. Strip "ADD " prefix from description
            var desc = r.Description;
            if (desc.StartsWith("ADD ", StringComparison.OrdinalIgnoreCase))
                desc = desc[4..];

            return new BlendAdditiveRowDto
            {
                OpCode = r.OpCode,
                Description = desc,
                Amount = smallAmount,
                Unit = smallUnit,
                Volume = r.Volume,
                Result = string.Empty,
                AmountDisplay = string.Empty
            };
        }).ToList();

        // 3. Group by opcode, sum amounts and volumes
        var grouped = processed
            .GroupBy(x => x.OpCode, StringComparer.OrdinalIgnoreCase)
            .Select(g => new BlendAdditiveRowDto
            {
                OpCode = g.Key,
                Description = g.First().Description,
                Amount = g.Sum(x => x.Amount),
                Unit = g.First().Unit,
                Volume = g.Sum(x => x.Volume),
                Result = string.Empty,
                AmountDisplay = string.Empty
            })
            .ToList();

        // 4. Normalize back to 1..1000
        foreach (var row in grouped)
        {
            var (normalizedAmount, normalizedUnit) = UnitConverter.Normalize(row.Amount, row.Unit);
            row.Amount = normalizedAmount;
            row.Unit = normalizedUnit;
        }

        // 5. Compute rate string and amount display
        foreach (var row in grouped)
        {
            var rate = row.Volume != 0
                ? row.Amount / row.Volume
                : row.Amount;

            var (rateValue, rateUnit) = UnitConverter.Normalize(rate, row.Unit);

            // Format: "XX.XXX UU/volUnitFirstChar"
            var volumeUnitChar = "L"; // gcvolunit — set from config or default
            row.Result = $"{rateValue,8:0.000} {rateUnit,-2}/" +
                         $"{volumeUnitChar[..1]}".PadLeft(2);
            row.Result = row.Result.PadLeft(15);

            row.AmountDisplay = $"{row.Amount,8:0.000} {row.Unit}".PadLeft(15);
        }

        return grouped;
    }

    private static IReadOnlyList<BlendCompositionRowDto> ApplyCompositionEnrichment(
        IReadOnlyList<BlendCompositionRowDto> rows)
    {
        if (rows.Count == 0) return rows;

        // Total litres for percentage calculation
        var total = rows.Sum(r => r.Litres);
        if (total == 0) return rows;

        foreach (var row in rows)
        {
            row.Percent = Math.Round((row.Litres / total) * 100m, 2);

            var blockPart = string.IsNullOrEmpty(row.Block)
                ? row.GeoId
                : row.Block;

            row.Legend = $"{blockPart.Trim()} - {row.Variety.Trim()} - {row.Vintage,4}";
        }

        return rows;
    }
}
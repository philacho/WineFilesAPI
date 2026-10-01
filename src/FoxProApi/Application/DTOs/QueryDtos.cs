using System.ComponentModel.DataAnnotations;

namespace WineFilesApi.Application.DTOs;

/// <summary>
/// Request for the blend/batch volume summary query.
/// All fields optional; omit to run without that filter.
/// </summary>
public sealed class BlendBatchSummaryQuery
{
    /// <summary>Optional blend code filter (exact match).</summary>
    [MaxLength(12)]
    public string? Blend { get; set; }

    /// <summary>Optional batch code filter (exact match).</summary>
    [MaxLength(8)]
    public string? Batch { get; set; }

    /// <summary>Optional minimum total volume.</summary>
    public decimal? MinVolume { get; set; }

    /// <summary>Optional max rows to return (default 500, max 5000).</summary>
    [Range(1, 5000)]
    public int Limit { get; set; } = 500;
}

/// <summary>
/// Request for the vessel/opmaster/opheader query.
/// </summary>
public sealed class VesselOpMasterQuery
{
    /// <summary>Lower capacity bound (lncplo).</summary>
    public decimal? CapacityLow { get; set; }

    /// <summary>Upper capacity bound (lncphi).</summary>
    public decimal? CapacityHigh { get; set; }

    /// <summary>Optional vessel filter.</summary>
    [MaxLength(12)]
    public string? Vessel { get; set; }

    /// <summary>Optional blend filter.</summary>
    [MaxLength(12)]
    public string? Blend { get; set; }

    /// <summary>Optional max rows (default 500, max 5000).</summary>
    [Range(1, 5000)]
    public int Limit { get; set; } = 500;
}
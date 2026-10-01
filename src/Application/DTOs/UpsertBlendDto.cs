using System.ComponentModel.DataAnnotations;

namespace WineFilesApi.Application.DTOs;

public sealed class UpsertBlendDto
{
    [Required, MaxLength(12)]
    public string Blend { get; set; } = string.Empty;

    [MaxLength(1)]
    public string Active { get; set; } = "Y";

    [MaxLength(1)]
    public string Compatibility { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string GeoId { get; set; } = string.Empty;

    [MaxLength(20)]
    public string GlCode { get; set; } = string.Empty;

    [MaxLength(8)]
    public string NameCode { get; set; } = string.Empty;

    [MaxLength(3)]
    public string UserLock { get; set; } = string.Empty;

    [MaxLength(5)]
    public string Variety { get; set; } = string.Empty;

    public int Vintage { get; set; }

    [MaxLength(12)]
    public string BlendGroup { get; set; } = string.Empty;

    [MaxLength(1)]
    public string Exception { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;
}

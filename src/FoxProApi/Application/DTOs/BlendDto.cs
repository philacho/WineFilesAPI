namespace WineFilesApi.Application.DTOs;

public sealed class BlendDto
{
    public string Blend { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string Compatibility { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GeoId { get; set; } = string.Empty;
    public string GlCode { get; set; } = string.Empty;
    public string NameCode { get; set; } = string.Empty;
    public string UserLock { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public int Vintage { get; set; }
    public string BlendGroup { get; set; } = string.Empty;
    public bool Exception { get; set; }
    public string Comments { get; set; } = string.Empty;
}

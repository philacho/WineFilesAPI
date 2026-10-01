namespace WineFilesApi.Domain.Entities;

public sealed class Blend
{
    public string BlendCode { get; set; } = string.Empty;
    public string Active { get; set; } = string.Empty;
    public string Compatibility { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GeoId { get; set; } = string.Empty;
    public string GlCode { get; set; } = string.Empty;
    public string NameCode { get; set; } = string.Empty;
    public string UserLock { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public int Vintage { get; set; }
    public string BlendGroup { get; set; } = string.Empty;
    public string ExceptionFlag { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}

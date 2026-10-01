namespace WineFilesApi.Application.DTOs;

public sealed class PmDbfDto
{
    public string Table { get; set; } = string.Empty;
    public string Cdx { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // R/W/S/I
    public string ParentTable { get; set; } = string.Empty;
    public string LinkField { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
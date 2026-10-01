namespace WineFilesApi.Application.DTOs;

public sealed class PmIndexDto
{
    public string CdxName { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string TagKey { get; set; } = string.Empty;
    public bool Unique { get; set; }
    public bool Descending { get; set; }
}
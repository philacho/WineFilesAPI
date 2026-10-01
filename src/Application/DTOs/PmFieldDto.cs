namespace WineFilesApi.Application.DTOs;

public sealed class PmFieldDto
{
    public string Table { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int Length { get; set; }
    public int Decimals { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
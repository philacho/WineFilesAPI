namespace WineFilesApi.Application.DTOs;

public sealed class BatchDto
{
    public string Batch { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string UserLock { get; set; } = string.Empty;
}

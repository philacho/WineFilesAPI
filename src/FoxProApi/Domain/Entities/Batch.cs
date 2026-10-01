namespace WineFilesApi.Domain.Entities;

public sealed class Batch
{
    public string BatchCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Active { get; set; } = string.Empty;
    public string UserLock { get; set; } = string.Empty;
}

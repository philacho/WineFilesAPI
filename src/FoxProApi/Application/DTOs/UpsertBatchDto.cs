using System.ComponentModel.DataAnnotations;

namespace WineFilesApi.Application.DTOs;

public sealed class UpsertBatchDto
{
    [Required, MaxLength(8)]
    public string Batch { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1)]
    public string Active { get; set; } = "Y";

    [MaxLength(3)]
    public string UserLock { get; set; } = string.Empty;
}

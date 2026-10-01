using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Services;

public sealed class PmFieldService : IPmFieldService
{
    private readonly IPmFieldRepository _repo;

    public PmFieldService(IPmFieldRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PmFieldDto>> GetAsync(string? table, CancellationToken ct)
        => (await _repo.GetAsync(table, ct)).Select(Map).ToList();

    public async Task<IReadOnlyList<PmFieldDto>> GetByTableAsync(string table, CancellationToken ct)
        => (await _repo.GetByTableAsync(table, ct)).Select(Map).ToList();

    private static PmFieldDto Map(PmField x) => new()
    {
        Table = x.TableName,
        Field = x.FieldName,
        DataType = x.DataType,
        Length = x.FieldLen,
        Decimals = x.Decimals,
        Prompt = x.ColPrompt,
        Title = x.ColTitle
    };
}
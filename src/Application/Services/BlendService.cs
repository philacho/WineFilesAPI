using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Services;

public sealed class BlendService : IBlendService
{
    private readonly IBlendRepository _repo;

    public BlendService(IBlendRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<BlendDto>> GetAsync(string? search, CancellationToken ct)
        => (await _repo.GetAsync(search, ct)).Select(Map).ToList();

    public async Task<BlendDto?> GetByIdAsync(string blend, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(blend, ct);
        return item is null ? null : Map(item);
    }

    public Task<bool> InsertAsync(UpsertBlendDto r, CancellationToken ct)
        => _repo.InsertAsync(ToEntity(r), ct);

    public Task<bool> UpdateAsync(string blend, UpsertBlendDto r, CancellationToken ct)
    {
        var item = ToEntity(r);
        item.BlendCode = blend.Trim();
        return _repo.UpdateAsync(item, ct);
    }

    public Task<bool> DeleteAsync(string blend, CancellationToken ct)
        => _repo.DeleteAsync(blend.Trim(), ct);

    private static Blend ToEntity(UpsertBlendDto r) => new() {
        BlendCode = r.Blend.Trim(),
        Active = r.Active.Trim(),
        Compatibility = r.Compatibility.Trim(),
        Description = r.Description.Trim(),
        GeoId = r.GeoId.Trim(),
        GlCode = r.GlCode.Trim(),
        NameCode = r.NameCode.Trim(),
        UserLock = r.UserLock.Trim(),
        Variety = r.Variety.Trim(),
        Vintage = r.Vintage,
        BlendGroup = r.BlendGroup.Trim(),
        ExceptionFlag = r.Exception.Trim(),
        Comments = r.Comments
    };

    private static BlendDto Map(Blend x) => new() {
        Blend = x.BlendCode,
        Active = x.Active.Equals("Y", StringComparison.OrdinalIgnoreCase),
        Compatibility = x.Compatibility,
        Description = x.Description,
        GeoId = x.GeoId,
        GlCode = x.GlCode,
        NameCode = x.NameCode,
        UserLock = x.UserLock,
        Variety = x.Variety,
        Vintage = x.Vintage,
        BlendGroup = x.BlendGroup,
        Exception = x.ExceptionFlag.Equals("Y", StringComparison.OrdinalIgnoreCase),
        Comments = x.Comments
    };
}

using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/blends")]
public sealed class BlendsController : ControllerBase
{
    private readonly IBlendService _service;

    public BlendsController(IBlendService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BlendDto>>> Get(
        [FromQuery] string? search,
        CancellationToken ct)
        => Ok(await _service.GetAsync(search, ct));

    [HttpGet("{blend}")]
    public async Task<ActionResult<BlendDto>> GetById(
        string blend, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(blend, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        UpsertBlendDto request, CancellationToken ct)
    {
        if (await _service.GetByIdAsync(request.Blend, ct) is not null)
            return Conflict($"Blend '{request.Blend}' already exists.");

        await _service.InsertAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { blend = request.Blend }, request);
    }

    [HttpPut("{blend}")]
    public async Task<IActionResult> Put(
        string blend, UpsertBlendDto request, CancellationToken ct)
    {
        if (await _service.GetByIdAsync(blend, ct) is null)
            return NotFound();

        await _service.UpdateAsync(blend, request, ct);
        return NoContent();
    }

    [HttpDelete("{blend}")]
    public async Task<IActionResult> Delete(string blend, CancellationToken ct)
        => await _service.DeleteAsync(blend, ct) ? NoContent() : NotFound();
}

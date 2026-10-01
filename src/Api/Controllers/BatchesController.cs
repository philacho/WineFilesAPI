using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/batches")]
public sealed class BatchesController(IBatchService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BatchDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? fields,
        CancellationToken ct)
        => Ok(await service.GetAsync(search, fields, ct));

    [HttpGet("{batch}")]
    public async Task<ActionResult<BatchDto>> GetById(
        string batch, [FromQuery] string? fields, CancellationToken ct)
    {
        var item = await service.GetByIdAsync(batch, fields, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UpsertBatchDto request, CancellationToken ct)
    {
        if (await service.GetByIdAsync(request.Batch, null, ct) is not null)
            return Conflict($"Batch '{request.Batch}' already exists.");
        await service.InsertAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { batch = request.Batch }, request);
    }

    [HttpPut("{batch}")]
    public async Task<IActionResult> Put(string batch, UpsertBatchDto request, CancellationToken ct)
    {
        if (await service.GetByIdAsync(batch, null, ct) is null) return NotFound();
        await service.UpdateAsync(batch, request, ct);
        return NoContent();
    }

    [HttpDelete("{batch}")]
    public async Task<IActionResult> Delete(string batch, CancellationToken ct)
        => await service.DeleteAsync(batch, ct) ? NoContent() : NotFound();
}
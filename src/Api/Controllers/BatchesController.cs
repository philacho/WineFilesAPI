using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/batches")]
public sealed class BatchesController : ControllerBase
{
    private readonly IBatchService _service;

    public BatchesController(IBatchService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BatchDto>>> Get(
        [FromQuery] string? search,
        CancellationToken ct)
        => Ok(await _service.GetAsync(search, ct));

    [HttpGet("{batch}")]
    public async Task<ActionResult<BatchDto>> GetById(
        string batch, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(batch, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        UpsertBatchDto request, CancellationToken ct)
    {
        if (await _service.GetByIdAsync(request.Batch, ct) is not null)
            return Conflict($"Batch '{request.Batch}' already exists.");

        await _service.InsertAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { batch = request.Batch }, request);
    }

    [HttpPut("{batch}")]
    public async Task<IActionResult> Put(
        string batch, UpsertBatchDto request, CancellationToken ct)
    {
        if (await _service.GetByIdAsync(batch, ct) is null)
            return NotFound();

        await _service.UpdateAsync(batch, request, ct);
        return NoContent();
    }

    [HttpDelete("{batch}")]
    public async Task<IActionResult> Delete(string batch, CancellationToken ct)
        => await _service.DeleteAsync(batch, ct) ? NoContent() : NotFound();
}

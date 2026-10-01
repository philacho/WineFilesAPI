using Microsoft.AspNetCore.Mvc;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/pm/indexes")]
public sealed class PmIndexController : ControllerBase
{
    private readonly IPmIndexService _service;

    public PmIndexController(IPmIndexService service) => _service = service;

    /// <summary>List all index tags, optionally filtered by CDX or tag name.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PmIndexDto>>> Get(
        [FromQuery] string? search, CancellationToken ct)
        => Ok(await _service.GetAsync(search, ct));

    /// <summary>List all index tags for a specific table (CDX).</summary>
    [HttpGet("by-table/{table}")]
    public async Task<ActionResult<IReadOnlyList<PmIndexDto>>> GetByTable(
        string table, CancellationToken ct)
        => Ok(await _service.GetByTableAsync(table, ct));
}
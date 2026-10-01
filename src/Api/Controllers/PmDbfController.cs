using Microsoft.AspNetCore.Mvc;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/pm/tables")]
public sealed class PmDbfController : ControllerBase
{
    private readonly IPmDbfService _service;

    public PmDbfController(IPmDbfService service) => _service = service;

    /// <summary>List all application tables, optionally filtered by name or description.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PmDbfDto>>> Get(
        [FromQuery] string? search, CancellationToken ct)
        => Ok(await _service.GetAsync(search, ct));

    /// <summary>Get one table's metadata by table name.</summary>
    [HttpGet("{table}")]
    public async Task<ActionResult<PmDbfDto>> GetById(
        string table, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(table, ct);
        return item is null ? NotFound() : Ok(item);
    }
}
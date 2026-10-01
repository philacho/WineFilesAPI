using Microsoft.AspNetCore.Mvc;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/pm/fields")]
public sealed class PmFieldController : ControllerBase
{
    private readonly IPmFieldService _service;

    public PmFieldController(IPmFieldService service) => _service = service;

    /// <summary>List all fields, optionally filtered by table name.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PmFieldDto>>> Get(
        [FromQuery] string? table, CancellationToken ct)
        => Ok(await _service.GetAsync(table, ct));

    /// <summary>List all fields for a specific table.</summary>
    [HttpGet("by-table/{table}")]
    public async Task<ActionResult<IReadOnlyList<PmFieldDto>>> GetByTable(
        string table, CancellationToken ct)
        => Ok(await _service.GetByTableAsync(table, ct));
}
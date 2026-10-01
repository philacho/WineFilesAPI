using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/queries")]
public sealed class QueriesController : ControllerBase
{
    private readonly IQueryService _service;

    public QueriesController(IQueryService service) => _service = service;

    /// <summary>
    /// Blend / opmaster / vessel volume summary grouped by blend and batch.
    /// </summary>
    [HttpPost("blend-batch-summary")]
    [ProducesResponseType(typeof(IReadOnlyList<IDictionary<string, object?>>), 200)]
    public async Task<IActionResult> BlendBatchSummary(
        [FromBody] BlendBatchSummaryQuery request, CancellationToken ct)
        => Ok(await _service.GetBlendBatchSummaryAsync(request, ct));

    /// <summary>
    /// Vessel / opmaster / opheader operation detail.
    /// </summary>
    [HttpPost("vessel-opmaster")]
    [ProducesResponseType(typeof(IReadOnlyList<IDictionary<string, object?>>), 200)]
    public async Task<IActionResult> VesselOpMaster(
        [FromBody] VesselOpMasterQuery request, CancellationToken ct)
        => Ok(await _service.GetVesselOpMasterAsync(request, ct));
}
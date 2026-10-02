using Microsoft.AspNetCore.Mvc;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/vessels")]
public sealed class VesselQueriesController(IVesselQueryService service) : ControllerBase
{
    /// <summary>
    /// Get the composition of a single vessel.
    /// Port of GetVesselComposition.
    /// </summary>
    [HttpGet("{vessel}/composition")]
    public async Task<ActionResult<IReadOnlyList<VesselCompositionRowDto>>> GetVesselComposition(
        string vessel,
        [FromQuery] int lastOp = 0,
        [FromQuery] bool fromCn = false,
        [FromQuery] string appLevel = "0",
        CancellationToken ct = default)
        => Ok(await service.GetVesselCompositionAsync(vessel, lastOp, fromCn, appLevel, ct));
}
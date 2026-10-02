using Microsoft.AspNetCore.Mvc;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/blends")]
public sealed class BlendQueriesController(IBlendQueryService service) : ControllerBase
{
    /// <summary>
    /// List vessels that hold the given blend.
    /// Port of GetBlendVessels.
    /// </summary>
    [HttpGet("{blend}/vessels")]
    public async Task<ActionResult<IReadOnlyList<BlendVesselDto>>> GetBlendVessels(
        string blend,
        [FromQuery] string? batch,
        [FromQuery] bool complete = false,
        CancellationToken ct = default)
        => Ok(await service.GetBlendVesselsAsync(blend, batch, complete, ct));

    /// <summary>
    /// Get the composition (variety/block/vintage breakdown) of the given blend.
    /// Port of GetBlendComposition.
    /// </summary>
    [HttpGet("{blend}/composition")]
    public async Task<ActionResult<IReadOnlyList<BlendCompositionRowDto>>> GetBlendComposition(
        string blend,
        [FromQuery] string? batch,
        [FromQuery] string appLevel = "0",
        CancellationToken ct = default)
        => Ok(await service.GetBlendCompositionAsync(blend, batch, appLevel, ct));

    /// <summary>
    /// Get the additives used in the given blend.
    /// Port of GetBlendAdditives.
    /// </summary>
    [HttpGet("{blend}/additives")]
    public async Task<ActionResult<IReadOnlyList<BlendAdditiveRowDto>>> GetBlendAdditives(
        string blend,
        [FromQuery] string? batch,
        [FromQuery] string appLevel = "0",
        CancellationToken ct = default)
        => Ok(await service.GetBlendAdditivesAsync(blend, batch, appLevel, ct));
}
using Microsoft.AspNetCore.Mvc;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DataController(IDataRepository repository) : ControllerBase
{
    // ---------- Existing POST JSON ----------
    [HttpPost("query")]
    public async Task<IActionResult> GetData(
        [FromBody] GetDataRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Table))
            return BadRequest("Table name is required.");

        if (request.Columns == null || request.Columns.Count == 0)
            return BadRequest("At least one column is required.");

        try
        {
            var result = await repository.GetDataAsync(
                request.Table, request.Columns, request.Filters, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ---------- NEW: free-form SQL (POST /api/Data/sql) ----------
    [HttpPost("sql")]
    public async Task<IActionResult> ExecuteSql(
        [FromBody] SqlQueryRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Sql))
            return BadRequest(new { error = "SQL is required." });

        // Cap the row count to a safe default
        var maxRows = request.MaxRows is > 0 and <= 10_000
            ? request.MaxRows.Value
            : 1_000;

        try
        {
            var result = await repository.ExecuteQueryAsync(request.Sql, maxRows, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
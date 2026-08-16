using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.DTOs.Logs;
using MyECommerceWebApp.Application.Interfaces;

namespace MyECommerceWebApp.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize(Policy = "Admin")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LogEventoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LogEventoDto>>> Get(
        [FromQuery] string? operacion,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _logService.GetRecentAsync(take, operacion, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(LogEventoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<LogEventoDto>> Registrar(
        [FromBody] RegistrarLogRequest request,
        CancellationToken cancellationToken)
    {
        var log = await _logService.RegistrarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { take = 1 }, log);
    }
}

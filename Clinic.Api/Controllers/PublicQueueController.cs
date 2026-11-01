using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Queue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

/// <summary>Public queue screens. Nothing here returns patient names or phone numbers.</summary>
[ApiController]
[Route("queue")]
[AllowAnonymous]
public class PublicQueueController : ControllerBase
{
    private readonly IQueueService _queue;

    public PublicQueueController(IQueueService queue) => _queue = queue;

    /// <summary>The waiting-room board: now serving, next up and waiting count for each doctor. Poll every 10 seconds.</summary>
    [HttpGet("board")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(QueueBoardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoard(CancellationToken ct) =>
        Ok(await _queue.GetBoardAsync(ct));

    /// <summary>Track My Queue: GET /queue/track/APT1006. Poll every 10 seconds.</summary>
    [HttpGet("track/{reference}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(TrackQueueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track(string reference, CancellationToken ct) =>
        Ok(await _queue.TrackAsync(reference, ct));
}

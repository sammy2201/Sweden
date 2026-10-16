using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SwedenStart;


[ApiController]
[Route("api/roadmap")]
[Authorize]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;

    public RoadmapController(IRoadmapService roadmapService)
        => _roadmapService = roadmapService;

    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoadmapResponseDto>> GenerateRoadmap(
        RoadmapRequestDto request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var response = await _roadmapService.GenerateRoadmapAsync(request, userId);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoadmapResponseDto>> GetRoadmap()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var response = await _roadmapService.GetRoadmapAsync(userId);
        return response == null
            ? NotFound(ApiProblemDetails.Create(StatusCodes.Status404NotFound, "Roadmap not found", "No roadmap exists for this user."))
            : Ok(response);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoadmapResponseDto>> UpdateRoadmap(
        RoadmapRequestDto request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var response = await _roadmapService.UpdateRoadmapAsync(request, userId);
        return Ok(response);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoadmap()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var deleted = await _roadmapService.DeleteRoadmapAsync(userId);
        return deleted
            ? NoContent()
            : NotFound(ApiProblemDetails.Create(StatusCodes.Status404NotFound, "Roadmap not found", "No roadmap exists for this user."));
    }

    [HttpPatch("tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoadmapResponseDto>> UpdateTaskCompletion(
        Guid taskId, UpdateTaskCompletionDto request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var response = await _roadmapService.SetTaskCompletionAsync(userId, taskId, request.Completed);
        return Ok(response);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        userId = Guid.Empty;
        return claim != null && Guid.TryParse(claim.Value, out userId);
    }
}
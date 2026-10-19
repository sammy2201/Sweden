
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SwedenStart;

[ApiController]
[Route("api/roadmap")]
[Authorize]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;
    private readonly RoadmapQuestionnaire _questionnaire;

    public RoadmapController(
        IRoadmapService roadmapService,
        RoadmapQuestionnaire questionnaire)
    {
        _roadmapService = roadmapService;
        _questionnaire = questionnaire;
    }

    [HttpGet("questions")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<RoadmapQuestionDto>> GetQuestionsController()
    {
        return Ok(_questionnaire.Questions
            .OrderBy(question => question.Order)
            .ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<RoadmapResponseDto>> GenerateRoadmapControllerAsync(
        RoadmapRequestDto request)
    {
        var validation = RoadmapAnswerValidator.Validate(request, _questionnaire);

        if (validation.Count > 0)
            return BadRequest(new ValidationProblemDetails(validation));

        var userId = GetUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var response = await _roadmapService.GenerateRoadmapServiceAsync(
            request, userId.Value);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<RoadmapResponseDto>> GetRoadmapControllerAsync()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var response = await _roadmapService.GetRoadmapServiceAsync(userId.Value);

        if (response == null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "No roadmap exists for this user.");
        }

        return Ok(response);
    }

    [HttpPut]
    public async Task<ActionResult<RoadmapResponseDto>> UpdateRoadmapControllerAsync(
        RoadmapRequestDto request)
    {
        var validation = RoadmapAnswerValidator.Validate(request, _questionnaire);

        if (validation.Count > 0)
            return BadRequest(new ValidationProblemDetails(validation));

        var userId = GetUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var response = await _roadmapService.UpdateRoadmapServiceAsync(
            request, userId.Value);

        return Ok(response);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteRoadmapControllerAsync()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var deleted = await _roadmapService.DeleteRoadmapServiceAsync(userId.Value);

        if (!deleted)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "No roadmap exists for this user.");
        }

        return NoContent();
    }

    [HttpPatch("tasks/{taskId:guid}")]
    public async Task<ActionResult<RoadmapResponseDto>> UpdateTaskCompletionControllerAsync(
        Guid taskId,
        UpdateTaskCompletionDto request)
    {
        if (request.Completed is not bool completed)
        {
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Completed)] =
                        ["The Completed field is required."]
                }));
        }

        var userId = GetUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var response = await _roadmapService.SetTaskCompletionServiceAsync(
            userId.Value, taskId, completed);

        return Ok(response);
    }

    // Gets the current user's ID from the authentication claims.
    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim != null && Guid.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}

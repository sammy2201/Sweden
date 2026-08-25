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
    public async Task<ActionResult<RoadmapResponseDto>> GenerateRoadmap(
        RoadmapRequestDto request)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return Unauthorized();

        try
        {
            var response = await _roadmapService.GenerateRoadmapAsync(request, userId);
            return Ok(response);
        }
        catch (RoadmapConflictException ex)
        {
            return Conflict(new
            {
                code = "ROADMAP_ALREADY_EXISTS",
                message = ex.Message,
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<RoadmapResponseDto>> GetRoadmap()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return Unauthorized();

        var response = await _roadmapService.GetRoadmapAsync(userId);
        return Ok(response);
    }

    [HttpPut]
    public async Task<ActionResult<RoadmapResponseDto>> UpdateRoadmap(
        RoadmapRequestDto request)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return Unauthorized();

        try
        {
            var response = await _roadmapService.UpdateRoadmapAsync(request, userId);
            return Ok(response);
        }
        catch (RoadmapNotFoundException ex)
        {
            return NotFound(new
            {
                code = "ROADMAP_NOT_FOUND",
                message = ex.Message,
            });
        }
    }
}
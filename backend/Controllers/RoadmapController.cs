using Microsoft.AspNetCore.Mvc;

namespace SwedenStart.Health;


[ApiController]
[Route("api/roadmap")]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;

    public RoadmapController(IRoadmapService roadmapService)
        => _roadmapService = roadmapService;

    [HttpPost]
    public async Task<ActionResult<RoadmapResponseDto>> GenerateRoadmap(
        RoadmapRequestDto request)
    {
        var response = await _roadmapService.GenerateRoadmapAsync(request);
        return Ok(response);
    }
}
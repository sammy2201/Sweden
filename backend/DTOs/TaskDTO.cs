using System.ComponentModel.DataAnnotations;

namespace SwedenStart;

public class RoadmapTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public string RuleId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string State { get; set; } = "Recommended";
    public string StateBeforeCompletion { get; set; } = "Recommended";
    public string? ResourceUrl { get; set; }
    public string RuleExplanation { get; set; } = string.Empty;
    public List<RoadmapTaskPrerequisite> Prerequisites { get; set; } = [];
}

public class UpdateTaskCompletionDto
{
    [Required]
    public bool? Completed { get; set; }
}

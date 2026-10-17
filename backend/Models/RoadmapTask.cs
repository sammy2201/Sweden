using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SwedenStart;

public sealed record RoadmapTaskPrerequisite(string RuleId, string Explanation);

public class RoadmapTask
{
    public Guid Id { get; set; }
    public Guid? RoadmapId { get; set; }
    public Roadmap? Roadmap { get; set; }
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
    public string PrerequisitesJson { get; set; } = "[]";

    [NotMapped]
    public List<RoadmapTaskPrerequisite> Prerequisites
    {
        get => JsonSerializer.Deserialize<List<RoadmapTaskPrerequisite>>(PrerequisitesJson) ?? [];
        set => PrerequisitesJson = JsonSerializer.Serialize(value);
    }
}

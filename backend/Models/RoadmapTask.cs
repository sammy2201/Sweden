namespace SwedenStart;

public class RoadmapTask
{
    public Guid Id { get; set; }
    public Guid? RoadmapId { get; set; }
    public Roadmap? Roadmap { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
}

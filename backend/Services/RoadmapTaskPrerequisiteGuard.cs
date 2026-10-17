namespace SwedenStart;

public static class RoadmapTaskPrerequisiteGuard
{
    public static IReadOnlyList<string> GetUnmetTitles(
        RoadmapTask task,
        IEnumerable<RoadmapTask> roadmapTasks)
    {
        var byRuleId = roadmapTasks
            .GroupBy(candidate => candidate.RuleId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        return task.Prerequisites
            .Where(prerequisite => byRuleId.TryGetValue(prerequisite.RuleId, out var prerequisiteTask)
                && prerequisiteTask.State != "NotApplicable"
                && !prerequisiteTask.Completed)
            .Select(prerequisite => byRuleId[prerequisite.RuleId].Title)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<string> GetCompletedDependentTitles(
        RoadmapTask task,
        IEnumerable<RoadmapTask> roadmapTasks)
    {
        return roadmapTasks
            .Where(candidate => candidate.RuleId != task.RuleId
                && candidate.Completed
                && candidate.State != "NotApplicable"
                && candidate.Prerequisites.Any(prerequisite => prerequisite.RuleId == task.RuleId))
            .Select(candidate => candidate.Title)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

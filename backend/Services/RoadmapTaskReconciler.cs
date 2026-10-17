namespace SwedenStart;

public static class RoadmapTaskReconciler
{
    private const string PublicTransportCardRuleId = "public-transport-card";
    private const string PublicTransportCardTitle = "Get a Public Transport Card";

    public static IReadOnlyList<RoadmapTask> Reconcile(
        ICollection<RoadmapTask> existing,
        IEnumerable<RoadmapTask> generated,
        Guid roadmapId)
    {
        var generatedByRule = generated.ToDictionary(task => task.RuleId, StringComparer.Ordinal);
        foreach (var oldTask in existing.ToArray())
        {
            RoadmapTask? replacement = null;
            if (!string.IsNullOrEmpty(oldTask.RuleId)
                && generatedByRule.Remove(oldTask.RuleId, out var matchingTask))
            {
                replacement = matchingTask;
            }

            if (replacement is null
                && string.Equals(oldTask.Title, PublicTransportCardTitle, StringComparison.OrdinalIgnoreCase)
                && generatedByRule.Remove(PublicTransportCardRuleId, out var transportCardTask))
            {
                replacement = transportCardTask;
            }

            if (replacement is null)
            {
                oldTask.State = "NotApplicable";
                continue;
            }
            oldTask.RuleId = replacement.RuleId;
            oldTask.Title = replacement.Title;
            oldTask.Description = replacement.Description;
            oldTask.Category = replacement.Category;
            oldTask.Priority = replacement.Priority;
            oldTask.StateBeforeCompletion = replacement.State;
            oldTask.Completed |= oldTask.State == "Completed";
            oldTask.State = oldTask.Completed ? "Completed" : replacement.State;
            oldTask.ResourceUrl = replacement.ResourceUrl;
            oldTask.RuleExplanation = replacement.RuleExplanation;
            oldTask.PrerequisitesJson = replacement.PrerequisitesJson;
        }
        var newTasks = generatedByRule.Values.ToArray();
        foreach (var task in newTasks)
        {
            task.Id = Guid.NewGuid();
            task.RoadmapId = roadmapId;
        }

        return newTasks;
    }
}

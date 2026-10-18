namespace SwedenStart;

public static class RoadmapTaskReconciler
{
    private const string PublicTransportCardRuleId = "public-transport-card";
    private const string PublicTransportCardTitle = "Get a Public Transport Card";

    public static IReadOnlyList<RoadmapTask> Reconcile(
        ICollection<RoadmapTask> existing,
        IEnumerable<RoadmapTask> generated,
        Guid roadmapId,
        bool preserveExistingState = false)
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
                oldTask.Completed |= oldTask.State == "Completed";
                oldTask.State = "NotApplicable";
                continue;
            }
            var wasCompleted = oldTask.Completed || oldTask.State == "Completed";
            var previousState = oldTask.State;
            oldTask.RuleId = replacement.RuleId;
            oldTask.Title = replacement.Title;
            oldTask.Description = replacement.Description;
            oldTask.Category = replacement.Category;
            oldTask.Priority = replacement.Priority;
            if (!preserveExistingState || previousState == "NotApplicable")
                oldTask.StateBeforeCompletion = replacement.State;
            oldTask.Completed = wasCompleted;
            oldTask.State = wasCompleted
                ? "Completed"
                : preserveExistingState && previousState != "NotApplicable"
                    ? previousState
                    : replacement.State;
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

    public static void ReconcileExistingRoadmap(Roadmap roadmap, RoadmapTaskGenerator generator)
    {
        var generatedTasks = generator.Generate(new RoadmapRequestDto
        {
            Origin = roadmap.Origin,
            ResidencePermit = roadmap.ResidencePermit,
            LiveInSweden = roadmap.LiveInSweden,
            Personnummer = roadmap.Personnummer,
            AppliedPersonnummer = roadmap.AppliedPersonnummer,
            IdCard = roadmap.IdCard,
            BankAccount = roadmap.BankAccount,
            BankId = roadmap.BankId,
            Housing = roadmap.Housing,
            PlanToDrive = roadmap.PlanToDrive,
            DrivingLicenceType = roadmap.DrivingLicenceType,
            Purpose = roadmap.Purpose,
            Insurance = roadmap.Insurance,
        });

        var newTasks = Reconcile(roadmap.Tasks, generatedTasks, roadmap.Id, preserveExistingState: true);
        roadmap.Tasks.AddRange(newTasks);
    }
}

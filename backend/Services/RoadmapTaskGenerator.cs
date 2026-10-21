namespace SwedenStart;

public class RoadmapTaskGenerator
{
    private readonly RoadmapTaskCatalog _catalog;

    public RoadmapTaskGenerator(RoadmapTaskCatalog catalog) => _catalog = catalog;

    public List<RoadmapTask> Generate(RoadmapRequestDto answers)
    {
        var answerValues = RoadmapQuestionnaire.Answers(answers);
        var hasNoResidencePermit = string.Equals(answers.ResidencePermit, "No", StringComparison.Ordinal);
        var tasks = _catalog.Rules
            .Where(rule => (!hasNoResidencePermit || rule.RuleId == "residence-status")
                && Matches(rule.When, answerValues))
            .Select(rule => CreateTask(rule, answers, answerValues))
            .ToList();

        if (tasks.Select(task => task.RuleId).Distinct(StringComparer.Ordinal).Count() != tasks.Count)
            throw new InvalidOperationException("Roadmap rules produced duplicate task IDs.");

        return RoadmapTaskOrdering.ByPriority(tasks);
    }

    private static RoadmapTask CreateTask(
        RoadmapTaskRuleConfiguration rule,
        RoadmapRequestDto answers,
        IReadOnlyDictionary<string, string> answerValues)
    {
        var variant = rule.Variants.FirstOrDefault(candidate => Matches(candidate.When, answerValues));
        return new RoadmapTask
        {
            RuleId = rule.RuleId,
            Title = variant?.Title ?? rule.Title,
            Description = Interpolate(variant?.Description ?? rule.Description, answers),
            Category = rule.Category,
            Priority = rule.Priority,
            State = rule.State,
            StateBeforeCompletion = rule.State,
            ResourceUrl = variant?.ResourceUrl ?? rule.ResourceUrl,
            RuleExplanation = rule.Explanation,
        };
    }

    private static bool Matches(
        IEnumerable<RoadmapAnswerConditionConfiguration> conditions,
        IReadOnlyDictionary<string, string> answers) =>
        conditions.All(condition => answers.TryGetValue(condition.QuestionId, out var value)
            && string.Equals(value, condition.Answer, StringComparison.Ordinal));

    private static string Interpolate(string value, RoadmapRequestDto answers) => value
        .Replace("{origin}", answers.Origin, StringComparison.Ordinal)
        .Replace("{purpose}", answers.Purpose, StringComparison.Ordinal);
}

public static class RoadmapTaskOrdering
{
    public static List<RoadmapTask> ByPriority(IEnumerable<RoadmapTask> source) => source
        .OrderBy(task => task.Priority)
        .ThenBy(task => task.RuleId, StringComparer.Ordinal)
        .ToList();
}

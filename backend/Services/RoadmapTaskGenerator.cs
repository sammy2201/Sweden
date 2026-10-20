namespace SwedenStart;

public class RoadmapTaskGenerator
{
    private readonly RoadmapTaskCatalog _catalog;

    public RoadmapTaskGenerator(RoadmapTaskCatalog catalog) => _catalog = catalog;

    public List<RoadmapTask> Generate(RoadmapRequestDto answers)
    {
        var answerValues = RoadmapQuestionnaire.Answers(answers);
        var tasks = _catalog.Rules
            .Where(rule => Matches(rule.When, answerValues))
            .Select(rule => CreateTask(rule, answers, answerValues))
            .ToList();

        if (tasks.Select(task => task.RuleId).Distinct(StringComparer.Ordinal).Count() != tasks.Count)
            throw new InvalidOperationException("Roadmap rules produced duplicate task IDs.");

        return RoadmapTaskOrdering.Topological(tasks);
    }

    private static RoadmapTask CreateTask(
        RoadmapTaskRuleConfiguration rule,
        RoadmapRequestDto answers,
        IReadOnlyDictionary<string, string> answerValues)
    {
        var variant = rule.Variants.FirstOrDefault(candidate => Matches(candidate.When, answerValues));
        var prerequisites = rule.Prerequisites
            .Where(prerequisite => Matches(prerequisite.When, answerValues))
            .Select(prerequisite => new RoadmapTaskPrerequisite(prerequisite.RuleId, prerequisite.Explanation))
            .ToList();

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
            Prerequisites = prerequisites,
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
    public static List<RoadmapTask> Topological(IEnumerable<RoadmapTask> source)
    {
        var tasks = source.ToDictionary(task => task.RuleId, StringComparer.Ordinal);
        var ordered = new List<RoadmapTask>();
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        void Visit(RoadmapTask task)
        {
            if (visited.Contains(task.RuleId)) return;
            if (!visiting.Add(task.RuleId)) throw new InvalidOperationException($"Roadmap task prerequisite cycle at '{task.RuleId}'.");
            foreach (var prerequisite in task.Prerequisites.Select(prerequisite => prerequisite.RuleId).Where(tasks.ContainsKey).Order(StringComparer.Ordinal))
                Visit(tasks[prerequisite]);
            visiting.Remove(task.RuleId);
            visited.Add(task.RuleId);
            ordered.Add(task);
        }

        foreach (var task in tasks.Values.OrderBy(roadmapTask => roadmapTask.Priority).ThenBy(roadmapTask => roadmapTask.RuleId, StringComparer.Ordinal)) Visit(task);
        return ordered;
    }
}

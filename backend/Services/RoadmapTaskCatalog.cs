using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwedenStart;

public class RoadmapTaskCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IReadOnlyList<RoadmapTaskRuleConfiguration> _rules;
    private readonly IReadOnlyList<RoadmapQuestionConfiguration> _questionnaire;

    private RoadmapTaskCatalog(
        IReadOnlyList<RoadmapQuestionConfiguration> questionnaire,
        IReadOnlyList<RoadmapTaskRuleConfiguration> rules)
    {
        _questionnaire = questionnaire;
        _rules = rules;
    }

    public IReadOnlyList<RoadmapQuestionConfiguration> Questionnaire => _questionnaire;
    public IReadOnlyList<RoadmapTaskRuleConfiguration> Rules => _rules;

    public static RoadmapTaskCatalog Load(string rulesPath, string questionsPath)
    {
        if (!File.Exists(rulesPath))
            throw new InvalidOperationException($"Roadmap task configuration was not found at '{rulesPath}'.");
        if (!File.Exists(questionsPath))
            throw new InvalidOperationException($"Roadmap question configuration was not found at '{questionsPath}'.");

        var rulesConfiguration = JsonSerializer.Deserialize<RoadmapTaskCatalogConfiguration>(File.ReadAllText(rulesPath), JsonOptions)
            ?? throw new InvalidOperationException("Roadmap task configuration is empty or invalid.");
        var questionsConfiguration = JsonSerializer.Deserialize<RoadmapQuestionnaireConfiguration>(File.ReadAllText(questionsPath), JsonOptions)
            ?? throw new InvalidOperationException("Roadmap question configuration is empty or invalid.");

        Validate(questionsConfiguration.Questions, rulesConfiguration.Rules);
        return new RoadmapTaskCatalog(questionsConfiguration.Questions, rulesConfiguration.Rules);
    }

    private static void Validate(
        IReadOnlyList<RoadmapQuestionConfiguration> questionnaire,
        IReadOnlyList<RoadmapTaskRuleConfiguration> rules)
    {
        var questionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var question in questionnaire)
            if (string.IsNullOrWhiteSpace(question.Id) || !questionIds.Add(question.Id))
                throw new InvalidOperationException($"Roadmap question ID '{question.Id}' is empty or duplicated.");

        var rulesById = new Dictionary<string, RoadmapTaskRuleConfiguration>(StringComparer.Ordinal);
        foreach (var rule in rules)
            if (string.IsNullOrWhiteSpace(rule.RuleId) || !rulesById.TryAdd(rule.RuleId, rule))
                throw new InvalidOperationException($"Roadmap task rule ID '{rule.RuleId}' is empty or duplicated.");

        foreach (var rule in rules)
        {
            foreach (var condition in rule.When
                .Concat(rule.Variants.SelectMany(variant => variant.When))
                .Concat(rule.Prerequisites.SelectMany(prerequisite => prerequisite.When)))
                if (!questionIds.Contains(condition.QuestionId))
                    throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' references unknown question '{condition.QuestionId}'.");
            foreach (var prerequisite in rule.Prerequisites)
                if (!rulesById.ContainsKey(prerequisite.RuleId))
                    throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' references unknown prerequisite '{prerequisite.RuleId}'.");
        }

        ValidateDependencyGraph(rules, rulesById);
    }

    private static void ValidateDependencyGraph(
        IReadOnlyList<RoadmapTaskRuleConfiguration> rules,
        IReadOnlyDictionary<string, RoadmapTaskRuleConfiguration> rulesById)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        void Visit(string ruleId)
        {
            if (visited.Contains(ruleId)) return;
            if (!visiting.Add(ruleId))
                throw new InvalidOperationException($"Roadmap task prerequisite cycle includes '{ruleId}'.");
            foreach (var prerequisiteId in rulesById[ruleId].Prerequisites.Select(p => p.RuleId))
                Visit(prerequisiteId);
            visiting.Remove(ruleId);
            visited.Add(ruleId);
        }

        foreach (var rule in rules) Visit(rule.RuleId);
    }
}

public class RoadmapTaskCatalogConfiguration
{
    public List<RoadmapTaskRuleConfiguration> Rules { get; set; } = [];
}

public class RoadmapQuestionnaireConfiguration
{
    public List<RoadmapQuestionConfiguration> Questions { get; set; } = [];
}

public class RoadmapQuestionConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string AnswerType { get; set; } = "select";
    public List<string> Values { get; set; } = [];
    public bool Required { get; set; }
    public int Order { get; set; }
    public bool AllowNotApplicable { get; set; }
    public List<RoadmapQuestionDependencyDto> Dependencies { get; set; } = [];
    public string? Guidance { get; set; }
}

public class RoadmapTaskRuleConfiguration
{
    public string RuleId { get; set; } = string.Empty;
    public List<RoadmapAnswerConditionConfiguration> When { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string State { get; set; } = "Recommended";
    public string? ResourceUrl { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<RoadmapTaskVariantConfiguration> Variants { get; set; } = [];
    public List<RoadmapTaskPrerequisiteConfiguration> Prerequisites { get; set; } = [];
}

public class RoadmapTaskVariantConfiguration
{
    public List<RoadmapAnswerConditionConfiguration> When { get; set; } = [];
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ResourceUrl { get; set; }
}

public class RoadmapTaskPrerequisiteConfiguration
{
    public string RuleId { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<RoadmapAnswerConditionConfiguration> When { get; set; } = [];
}

public class RoadmapAnswerConditionConfiguration
{
    public string QuestionId { get; set; } = string.Empty;
    [JsonPropertyName("equals")]
    public string Answer { get; set; } = string.Empty;
}

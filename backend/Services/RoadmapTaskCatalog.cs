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
        if (questionnaire.Count == 0)
            throw new InvalidOperationException("Roadmap questionnaire configuration must define at least one question.");
        if (rules.Count == 0)
            throw new InvalidOperationException("Roadmap task configuration must define at least one rule.");

        var questionsById = new Dictionary<string, RoadmapQuestionConfiguration>(StringComparer.Ordinal);
        var questionOrders = new HashSet<int>();
        foreach (var question in questionnaire)
        {
            if (string.IsNullOrWhiteSpace(question.Id) || !questionsById.TryAdd(question.Id, question))
                throw new InvalidOperationException($"Roadmap question ID '{question.Id}' is empty or duplicated.");
            if (string.IsNullOrWhiteSpace(question.Text) || question.Order < 1 || !questionOrders.Add(question.Order)
                || question.AnswerType != "select")
                throw new InvalidOperationException($"Roadmap question '{question.Id}' has invalid or incomplete question metadata.");
            if (question.Values.Count == 0 || question.Values.Any(string.IsNullOrWhiteSpace)
                || question.Values.Distinct(StringComparer.Ordinal).Count() != question.Values.Count)
                throw new InvalidOperationException($"Roadmap question '{question.Id}' must define unique, non-empty answer options.");
        }

        var expectedQuestionIds = RoadmapQuestionnaire.Answers(new RoadmapRequestDto()).Keys.ToHashSet(StringComparer.Ordinal);
        if (!expectedQuestionIds.SetEquals(questionsById.Keys))
            throw new InvalidOperationException("Roadmap questionnaire questions do not match the answer fields handled by the C# mapper.");
        ValidateQuestionDependencies(questionnaire, questionsById);
        var rulesById = new Dictionary<string, RoadmapTaskRuleConfiguration>(StringComparer.Ordinal);

        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleId) || !rulesById.TryAdd(rule.RuleId, rule))
                throw new InvalidOperationException($"Roadmap task rule ID '{rule.RuleId}' is empty or duplicated.");
            if (string.IsNullOrWhiteSpace(rule.Title) || string.IsNullOrWhiteSpace(rule.Description)
                || string.IsNullOrWhiteSpace(rule.Category) || string.IsNullOrWhiteSpace(rule.Explanation)
                || string.IsNullOrWhiteSpace(rule.State))
                throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' is missing required task text.");
            if (rule.Priority < 1)
                throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' must have a positive priority.");

            ValidateConditions(rule.RuleId, rule.When, questionsById);
            foreach (var variant in rule.Variants)
            {
                ValidateConditions(rule.RuleId, variant.When, questionsById);
                if (variant.Title is not null && string.IsNullOrWhiteSpace(variant.Title)
                    || variant.Description is not null && string.IsNullOrWhiteSpace(variant.Description))
                    throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' has an empty variant value.");
            }

            foreach (var prerequisite in rule.Prerequisites)
            {
                if (string.IsNullOrWhiteSpace(prerequisite.RuleId) || string.IsNullOrWhiteSpace(prerequisite.Explanation))
                    throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' has an incomplete prerequisite.");
                ValidateConditions(rule.RuleId, prerequisite.When, questionsById);
            }
        }

        foreach (var rule in rules)
            foreach (var prerequisite in rule.Prerequisites)
                if (!rulesById.ContainsKey(prerequisite.RuleId))
                    throw new InvalidOperationException($"Roadmap task rule '{rule.RuleId}' references unknown prerequisite '{prerequisite.RuleId}'.");

        ValidateDependencyGraph(rules, rulesById);
    }

    private static void ValidateConditions(
        string ruleId,
        IReadOnlyList<RoadmapAnswerConditionConfiguration> conditions,
        IReadOnlyDictionary<string, RoadmapQuestionConfiguration> questionsById)
    {
        foreach (var condition in conditions)
        {
            if (!questionsById.TryGetValue(condition.QuestionId, out var question))
                throw new InvalidOperationException($"Roadmap task rule '{ruleId}' references unknown question '{condition.QuestionId}'.");
            if (!question.Values.Contains(condition.Answer, StringComparer.Ordinal)
                && !(question.AllowNotApplicable && condition.Answer == "Not applicable"))
                throw new InvalidOperationException($"Roadmap task rule '{ruleId}' uses unknown answer '{condition.Answer}' for question '{condition.QuestionId}'.");
        }
    }

    private static void ValidateQuestionDependencies(
        IReadOnlyList<RoadmapQuestionConfiguration> questions,
        IReadOnlyDictionary<string, RoadmapQuestionConfiguration> questionsById)
    {
        var dependencyIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var question in questions)
            foreach (var dependency in question.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency.Id) || !dependencyIds.Add(dependency.Id))
                    throw new InvalidOperationException($"Roadmap question dependency ID '{dependency.Id}' is empty or duplicated.");
                if (dependency.DependentQuestionId != question.Id)
                    throw new InvalidOperationException($"Roadmap question dependency '{dependency.Id}' targets a different question than '{question.Id}'.");
                if (!questionsById.TryGetValue(dependency.SourceQuestionId, out var sourceQuestion))
                    throw new InvalidOperationException($"Roadmap question dependency '{dependency.Id}' references unknown source question '{dependency.SourceQuestionId}'.");
                if (!sourceQuestion.Values.Contains(dependency.Answer, StringComparer.Ordinal))
                    throw new InvalidOperationException($"Roadmap question dependency '{dependency.Id}' uses unknown answer '{dependency.Answer}'.");
                if (!Enum.TryParse<RoadmapDependencyOperator>(dependency.Operator, ignoreCase: false, out var dependencyOperator)
                    || !Enum.IsDefined(dependencyOperator)
                    || !Enum.TryParse<RoadmapDependencyEffect>(dependency.Effect, ignoreCase: false, out var dependencyEffect)
                    || !Enum.IsDefined(dependencyEffect))
                    throw new InvalidOperationException($"Roadmap question dependency '{dependency.Id}' uses an unsupported operator or effect.");
            }
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

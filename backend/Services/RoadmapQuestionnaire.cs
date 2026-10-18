namespace SwedenStart;

public enum RoadmapDependencyOperator { Equals }
public enum RoadmapDependencyEffect { Show, Hide, RequireClarification }

public record RoadmapQuestionOptionDto(string Id, string Value, string Label);
public record RoadmapQuestionDependencyDto(
    string Id,
    string SourceQuestionId,
    string Operator,
    string Answer,
    string DependentQuestionId,
    string Effect,
    string Explanation);
public record RoadmapQuestionDto(
    string Id,
    string Text,
    string AnswerType,
    IReadOnlyList<RoadmapQuestionOptionDto> Options,
    bool Required,
    int Order,
    IReadOnlyList<RoadmapQuestionDependencyDto> Dependencies,
    string? Guidance = null);

public class RoadmapQuestionnaire
{
    private const string NotApplicable = "Not applicable";
    private readonly IReadOnlyDictionary<string, RoadmapQuestionConfiguration> _configurationById;

    public RoadmapQuestionnaire(RoadmapTaskCatalog catalog)
    {
        _configurationById = catalog.Questionnaire.ToDictionary(question => question.Id, StringComparer.Ordinal);
        Questions = catalog.Questionnaire.Select(question => new RoadmapQuestionDto(
            question.Id,
            question.Text,
            question.AnswerType,
            question.Values.Select(value => new RoadmapQuestionOptionDto(
                $"{question.Id}-{value.ToLowerInvariant().Replace(' ', '-')}", value, value)).ToArray(),
            question.Required,
            question.Order,
            question.Dependencies,
            question.Guidance)).ToArray();
    }

    public IReadOnlyList<RoadmapQuestionDto> Questions { get; }

    public bool IsAllowedValue(string questionId, string value) =>
        _configurationById.TryGetValue(questionId, out var question)
        && (question.Values.Contains(value, StringComparer.Ordinal)
            || question.AllowNotApplicable && value == NotApplicable);

    public static IReadOnlyDictionary<string, string> Answers(RoadmapRequestDto a) => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["origin"] = a.Origin,
        ["residencePermit"] = a.ResidencePermit,
        ["liveInSweden"] = a.LiveInSweden,
        ["personnummer"] = a.Personnummer,
        ["appliedPersonnummer"] = a.AppliedPersonnummer,
        ["idCard"] = a.IdCard,
        ["bankAccount"] = a.BankAccount,
        ["bankId"] = a.BankId,
        ["housing"] = a.Housing,
        ["planToDrive"] = a.PlanToDrive,
        ["drivingLicenceType"] = a.DrivingLicenceType,
        ["purpose"] = a.Purpose,
        ["insurance"] = a.Insurance,
    };

    public bool IsVisible(RoadmapQuestionDto question, IReadOnlyDictionary<string, string> answers)
    {
        var rules = question.Dependencies.Where(d => d.Effect is "Show" or "Hide").ToArray();
        bool Matches(RoadmapQuestionDependencyDto dependency) =>
            dependency.Operator == RoadmapDependencyOperator.Equals.ToString() &&
            answers.TryGetValue(dependency.SourceQuestionId, out var value) &&
            string.Equals(value, dependency.Answer, StringComparison.Ordinal);
        var showRules = rules.Where(d => d.Effect == "Show").ToArray();
        return (showRules.Length == 0 || showRules.Any(Matches)) && !rules.Where(d => d.Effect == "Hide").Any(Matches);
    }
}

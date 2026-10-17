namespace SwedenStart;

public enum RoadmapDependencyOperator { Equals }
public enum RoadmapDependencyEffect { Show, Hide, RequireClarification }

public sealed record RoadmapQuestionOptionDto(string Id, string Value, string Label);
public sealed record RoadmapQuestionDependencyDto(
    string Id,
    string SourceQuestionId,
    string Operator,
    string Answer,
    string DependentQuestionId,
    string Effect,
    string Explanation);
public sealed record RoadmapQuestionDto(
    string Id,
    string Text,
    string AnswerType,
    IReadOnlyList<RoadmapQuestionOptionDto> Options,
    bool Required,
    int Order,
    IReadOnlyList<RoadmapQuestionDependencyDto> Dependencies,
    string? Guidance = null);

public static class RoadmapQuestionnaire
{
    private static RoadmapQuestionDependencyDto D(string id, string source, string answer, string dependent, string explanation) =>
        new(id, source, RoadmapDependencyOperator.Equals.ToString(), answer, dependent,
            RoadmapDependencyEffect.Show.ToString(), explanation);

    private static RoadmapQuestionDto Q(string id, string text, string[] values, int order,
        string? guidance = null, params RoadmapQuestionDependencyDto[] dependencies) =>
        new(id, text, "select", values.Select(v => new RoadmapQuestionOptionDto(
            $"{id}-{v.ToLowerInvariant().Replace(' ', '-')}", v, v)).ToArray(), true, order, dependencies, guidance);

    public static IReadOnlyList<RoadmapQuestionDto> Questions { get; } =
    [
        Q("origin", "Where are you from?", ["EU", "Non-EU"], 1,
            "These choices do not separately identify EEA or Nordic status. Nationality alone does not establish residence rights."),
        Q("residencePermit", "Do you already have a residence permit?", ["Yes", "No"], 2,
            "A No answer does not by itself establish that a permit is required; individual residence rights depend on more information.",
            new RoadmapQuestionDependencyDto("clarify-residence-rights-from-origin", "origin", "Equals", "Non-EU", "residencePermit", "RequireClarification", "Origin alone does not determine residence rights or whether a permit is required; check rules for your individual situation and purpose.")),
        Q("liveInSweden", "Do you already live in Sweden?", ["Yes", "No"], 3),
        Q("personnummer", "Do you have a Personnummer?", ["Yes", "No"], 4,
            dependencies: [D("show-personnummer-for-residents", "liveInSweden", "Yes", "personnummer", "Ask about registration only when the respondent already lives in Sweden.")]),
        Q("appliedPersonnummer", "Have you applied for a Personnummer?", ["Yes", "No", "Waiting for decision"], 5,
            dependencies: [D("show-personnummer-application-status", "personnummer", "No", "appliedPersonnummer", "The application status only applies when the respondent does not already have a Personnummer.")]),
        Q("idCard", "Do you have a Swedish ID card?", ["Yes", "No"], 6,
            dependencies: [D("show-id-card-for-residents", "liveInSweden", "Yes", "idCard", "Ask about identity documentation for respondents already living in Sweden.")]),
        Q("bankAccount", "Do you have a Swedish bank account?", ["Yes", "No"], 7,
            dependencies: [D("show-bank-account-for-residents", "liveInSweden", "Yes", "bankAccount", "Bank requirements vary; the answer is useful even when the respondent lacks a Personnummer or Swedish ID card.")]),
        Q("bankId", "Do you have BankID?", ["Yes", "No"], 8,
            dependencies: [D("show-bankid-for-residents", "liveInSweden", "Yes", "bankId", "BankID eligibility depends on the provider and is not guaranteed by holding a bank account.")]),
        Q("housing", "What is your housing situation?", ["Permanent", "Temporary", "Still looking"], 9),
        Q("planToDrive", "Do you plan to drive in Sweden?", ["Yes", "No"], 10),
        Q("drivingLicenceType", "Which licence do you have?", ["Swedish", "EU", "Other"], 11,
            "This answer describes the licence category, not its issuing country. Origin does not establish which driving rules apply.",
            D("show-licence-if-driving", "planToDrive", "Yes", "drivingLicenceType", "Licence information is only relevant when the respondent plans to drive.")),
        Q("purpose", "Why are you in Sweden?", ["Work", "Study", "Looking for work", "Family"], 12),
        Q("insurance", "Have you registered with Försäkringskassan?", ["Yes", "No"], 13,
            dependencies: [D("show-insurance-for-residents", "liveInSweden", "Yes", "insurance", "Ask about Swedish social-insurance registration only for respondents already living in Sweden.")]),
    ];

    public static IReadOnlyDictionary<string, string> Answers(RoadmapRequestDto a) => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["origin"] = a.Origin, ["residencePermit"] = a.ResidencePermit, ["liveInSweden"] = a.LiveInSweden,
        ["personnummer"] = a.Personnummer, ["appliedPersonnummer"] = a.AppliedPersonnummer,
        ["idCard"] = a.IdCard, ["bankAccount"] = a.BankAccount, ["bankId"] = a.BankId,
        ["housing"] = a.Housing, ["planToDrive"] = a.PlanToDrive,
        ["drivingLicenceType"] = a.DrivingLicenceType, ["purpose"] = a.Purpose, ["insurance"] = a.Insurance,
    };

    public static bool IsVisible(RoadmapQuestionDto question, IReadOnlyDictionary<string, string> answers)
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

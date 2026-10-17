namespace SwedenStart;

public static class RoadmapAnswerValidator
{
    private const string NotApplicable = "Not applicable";

    private static readonly IReadOnlyDictionary<string, string[]> Allowed = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["origin"] = ["EU", "Non-EU"], ["residencePermit"] = ["Yes", "No"],
        ["liveInSweden"] = ["Yes", "No"], ["personnummer"] = ["Yes", "No", NotApplicable],
        ["appliedPersonnummer"] = ["Yes", "No", "Waiting for decision", NotApplicable],
        ["idCard"] = ["Yes", "No", NotApplicable], ["bankAccount"] = ["Yes", "No", NotApplicable],
        ["bankId"] = ["Yes", "No", NotApplicable], ["housing"] = ["Permanent", "Temporary", "Still looking"],
        ["planToDrive"] = ["Yes", "No"], ["drivingLicenceType"] = ["Swedish", "EU", "Other", NotApplicable],
        ["purpose"] = ["Work", "Study", "Looking for work", "Family"], ["insurance"] = ["Yes", "No", NotApplicable],
    };

    public static Dictionary<string, string[]> Validate(RoadmapRequestDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var answers = RoadmapQuestionnaire.Answers(request);
        foreach (var (questionId, value) in answers)
            if (!Allowed[questionId].Contains(value, StringComparer.Ordinal))
                errors[questionId] = [$"Select a valid {questionId} option."];

        foreach (var question in RoadmapQuestionnaire.Questions)
        {
            if (!answers.TryGetValue(question.Id, out var value) || errors.ContainsKey(question.Id)) continue;
            if (RoadmapQuestionnaire.IsVisible(question, answers) && value == NotApplicable)
                errors[question.Id] = [$"{question.Text} is required for your answers."];
            else if (!RoadmapQuestionnaire.IsVisible(question, answers) && value != NotApplicable)
                errors[question.Id] = [$"{question.Text} is not applicable based on your earlier answers."];
        }

        if (request.Personnummer == "Yes" && request.AppliedPersonnummer != NotApplicable)
            errors["appliedPersonnummer"] = ["An application is not applicable when you already have a Personnummer."];
        if (request.Personnummer == "No" && request.LiveInSweden == "Yes" && request.AppliedPersonnummer == NotApplicable)
            errors["appliedPersonnummer"] = ["Say whether you applied or are waiting for a decision."];
        if (request.BankAccount == "No" && request.BankId == "Yes")
            errors["bankId"] = ["BankID cannot be reported without a Swedish bank account."];

        return errors;
    }
}

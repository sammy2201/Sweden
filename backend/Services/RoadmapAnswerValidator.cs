namespace SwedenStart;

public static class RoadmapAnswerValidator
{
    private const string NotApplicable = "Not applicable";

    public static Dictionary<string, string[]> Validate(RoadmapRequestDto request, RoadmapQuestionnaire questionnaire)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var answers = RoadmapQuestionnaire.Answers(request);
        foreach (var (questionId, value) in answers)
            if (!questionnaire.IsAllowedValue(questionId, value))
                errors[questionId] = [$"Select a valid {questionId} option."];

        foreach (var question in questionnaire.Questions)
        {
            if (!answers.TryGetValue(question.Id, out var value) || errors.ContainsKey(question.Id)) continue;
            var isVisible = questionnaire.IsVisible(question, answers);
            if (isVisible && value == NotApplicable)
                errors[question.Id] = [$"{question.Text} is required for your answers."];
            else if (!isVisible && value != NotApplicable)
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

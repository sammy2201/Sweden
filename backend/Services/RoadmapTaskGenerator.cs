namespace SwedenStart;

public sealed record RoadmapTaskRule(
    string RuleId,
    IReadOnlyList<string> AnswerQuestionIds,
    Func<RoadmapRequestDto, bool> Applies,
    Func<RoadmapRequestDto, RoadmapTask> CreateTask,
    string Explanation);

public sealed class RoadmapTaskGenerator
{
    private static RoadmapTask Create(string id, string title, string description, string category,
        int priority, string state = "Recommended", string? resourceUrl = null,
        params RoadmapTaskPrerequisite[] prerequisites) =>
        new()
        {
            RuleId = id, Title = title, Description = description, Category = category,
            Priority = priority, State = state, StateBeforeCompletion = state, ResourceUrl = resourceUrl,
            Prerequisites = prerequisites.ToList(),
        };

    private static RoadmapTask CreateBankIdGuidance(RoadmapRequestDto answers)
    {
        var prerequisites = new List<RoadmapTaskPrerequisite>();
        if (answers.BankAccount == "No")
            prerequisites.Add(new("bank-account", "BankID is issued through banks; prepare to discuss account-opening requirements, but account eligibility does not guarantee BankID eligibility."));
        if (answers.Personnummer == "No")
        {
            var personnummerRule = answers.AppliedPersonnummer switch
            {
                "Waiting for decision" => "personnummer-pending",
                "Yes" => "personnummer-status",
                _ => "personnummer-prepare",
            };
            prerequisites.Add(new(personnummerRule, "BankID's current eligibility guidance requires a Swedish personal identity number; confirm your circumstances with an issuing bank."));
        }
        return Create("bankid", "Check BankID eligibility and setup options", "Ask an issuing bank about eligibility and setup requirements. Having a bank account does not guarantee BankID; ask about alternative access methods.", "Banking", 3, "Recommended", "https://www.bankid.com/en/individuals/get-bankid", prerequisites.ToArray());
    }

    public static IReadOnlyList<RoadmapTaskRule> Rules { get; } =
    [
        new("settle-arrival", ["liveInSweden"], a => a.LiveInSweden == "No",
            _ => Create("settle-arrival", "Prepare for arrival", "Review practical steps for settling in Sweden before or soon after arrival.", "Settlement", 1, "Applicable"),
            "The respondent does not yet live in Sweden."),
        new("residence-status", ["origin", "residencePermit", "purpose"], a => a.ResidencePermit == "No",
            a => Create("residence-status", "Review residence guidance for your situation", $"Review guidance for the selected origin category ({a.Origin}) and purpose ({a.Purpose}). These answers alone do not establish whether you need a permit or qualify to live in Sweden.", "Residence", 2, resourceUrl: "https://www.migrationsverket.se/en/you-want-to-apply.html"),
            "The respondent has no current residence permit; origin and purpose may help find relevant guidance but are not enough to determine individual residence rights."),
        new("personnummer-prepare", ["liveInSweden", "personnummer", "appliedPersonnummer"], a => a.LiveInSweden == "Yes" && a.Personnummer == "No" && a.AppliedPersonnummer == "No",
            _ => Create("personnummer-prepare", "Check eligibility and prepare a Personnummer application", "Review Skatteverket's current requirements for your situation and prepare the supporting documents before applying.", "Registration", 1, "Applicable", "https://www.skatteverket.se/servicelankar/otherlanguages/englishengelska/individualsandemployees/movingtosweden.4.7be5268414bea064694c40c.html"),
            "The respondent lives in Sweden, has no Personnummer, and has not applied."),
        new("personnummer-status", ["liveInSweden", "personnummer", "appliedPersonnummer"], a => a.LiveInSweden == "Yes" && a.Personnummer == "No" && a.AppliedPersonnummer == "Yes",
            _ => Create("personnummer-status", "Check your Personnummer application status", "If you have not received a decision, check the status with Skatteverket. Do not submit a duplicate application.", "Registration", 2, resourceUrl: "https://www.skatteverket.se/servicelankar/otherlanguages/englishengelska/individualsandemployees/movingtosweden.4.7be5268414bea064694c40c.html"),
            "The respondent has applied but did not say a decision is pending; check status without assuming the application is still waiting."),
        new("personnummer-pending", ["liveInSweden", "personnummer", "appliedPersonnummer"], a => a.LiveInSweden == "Yes" && a.Personnummer == "No" && a.AppliedPersonnummer == "Waiting for decision",
            _ => Create("personnummer-pending", "Follow your Personnummer application", "Wait for the decision and contact Skatteverket if you need an update. Do not submit a duplicate application.", "Registration", 1, "Waiting", "https://www.skatteverket.se/servicelankar/otherlanguages/englishengelska/individualsandemployees/movingtosweden.4.7be5268414bea064694c40c.html"),
            "The respondent explicitly says a decision is pending."),
        new("id-card", ["liveInSweden", "idCard", "personnummer"], a => a.LiveInSweden == "Yes" && a.IdCard == "No",
            a => a.Personnummer == "No"
                ? Create("id-card", "Check Swedish ID card eligibility", "Ask Skatteverket which identity documents and personal-number status apply to your circumstances before preparing an application.", "Identity", 2, "Recommended", "https://www.skatteverket.se/servicelankar/otherlanguages/inenglishengelska/individualsandemployees/livinginsweden/swedishtaxagencyidentitycards/howtoapplyfororrenewidcard.4.3810a01c150939e893f1e20d.html", [new(a.AppliedPersonnummer switch { "Waiting for decision" => "personnummer-pending", "Yes" => "personnummer-status", _ => "personnummer-prepare" }, "A Personnummer may be relevant to the ID card process; confirm eligibility and timing with Skatteverket.")])
                : Create("id-card", "Check Swedish ID card eligibility and application steps", "Review Skatteverket's current requirements before applying; eligibility is assessed for your circumstances.", "Identity", 2, resourceUrl: "https://www.skatteverket.se/servicelankar/otherlanguages/inenglishengelska/individualsandemployees/livinginsweden/swedishtaxagencyidentitycards/howtoapplyfororrenewidcard.4.3810a01c150939e893f1e20d.html"),
            "A missing Swedish ID card calls for eligibility and application guidance, not an assumed legal obligation."),
        new("bank-account", ["liveInSweden", "bankAccount"], a => a.LiveInSweden == "Yes" && a.BankAccount == "No",
            _ => Create("bank-account", "Prepare to open a bank account", "Compare banks' current identity, documentation, and appointment requirements. A Personnummer or Swedish ID card does not guarantee eligibility.", "Banking", 2, "Applicable"),
            "The respondent does not have a Swedish bank account; requirements vary between banks."),
        new("bankid", ["liveInSweden", "bankId", "bankAccount"], a => a.LiveInSweden == "Yes" && a.BankId == "No",
            CreateBankIdGuidance,
            "The respondent does not have BankID; provider eligibility and prerequisites must be confirmed."),
        new("insurance-review", ["liveInSweden", "insurance"], a => a.LiveInSweden == "Yes" && a.Insurance == "No",
            _ => Create("insurance-review", "Review your social insurance and coverage", "Check your registration and coverage situation with Försäkringskassan. Separately consider whether additional insurance suits your needs; private cover is not a universal legal requirement.", "Insurance", 3, resourceUrl: "https://www.forsakringskassan.se/english/moving-to-working-studying-or-newly-arrived-in-sweden/social-insurance-in-sweden/insured-in-sweden"),
            "The answer indicates no Försäkringskassan registration; it does not establish that the respondent lacks all insurance coverage."),
        new("housing-search", ["housing"], a => a.Housing == "Still looking",
            _ => Create("housing-search", "Find housing and review rental terms", "Compare housing options and understand the rental contract, deposit, and notice terms before signing.", "Housing", 1, "Applicable"),
            "The respondent is still looking for housing."),
        new("housing-next", ["housing"], a => a.Housing == "Temporary",
            _ => Create("housing-next", "Plan your next housing step", "Review your current arrangement and decide whether you need longer-term accommodation.", "Housing", 3),
            "The respondent has temporary housing, so a follow-up plan may be useful."),
        new("public-transport-card", [], _ => true,
            _ => Create("public-transport-card", "Get a Public Transport Card", "Purchase a regional travel card for the area where you live or travel. No other roadmap steps are required first.", "Transport", 4, "Applicable"),
            "A regional travel card can be purchased independently of the respondent's other roadmap answers."),
        new("licence-eu", ["planToDrive", "drivingLicenceType"], a => a.PlanToDrive == "Yes" && a.DrivingLicenceType == "EU",
            _ => Create("licence-eu", "Check rules for your EU licence", "Confirm the rules and conditions for your particular licence and issuing country with Transportstyrelsen.", "Transport", 3, resourceUrl: "https://www.transportstyrelsen.se/en/road/driving-licences/"),
            "The respondent plans to drive and selected the EU licence category; the issuing country and individual rules still need confirmation."),
        new("licence-other", ["planToDrive", "drivingLicenceType"], a => a.PlanToDrive == "Yes" && a.DrivingLicenceType == "Other",
            _ => Create("licence-other", "Check rules for your driving licence", "Ask Transportstyrelsen whether your particular licence can be used or exchanged in Sweden; rules vary by issuing country and circumstances.", "Transport", 3, resourceUrl: "https://www.transportstyrelsen.se/en/road/driving-licences/"),
            "The respondent plans to drive and selected Other; no exchange or eligibility outcome is assumed."),
        new("licence-swedish", ["planToDrive", "drivingLicenceType"], a => a.PlanToDrive == "Yes" && a.DrivingLicenceType == "Swedish",
            _ => Create("licence-swedish", "Check your Swedish driving licence details", "Keep your licence accessible and check that its details are current.", "Transport", 4),
            "The respondent plans to drive and reports having a Swedish licence."),
        new("work-search", ["purpose"], a => a.Purpose == "Looking for work",
            _ => Create("work-search", "Review job-seeking support", "Review Arbetsförmedlingen's services and create a profile if appropriate for your situation.", "Work", 2, resourceUrl: "https://arbetsformedlingen.se/other-languages/english-engelska/find-your-new-job-in-sweden"),
            "The respondent's stated purpose is looking for work."),
        new("study-planning", ["purpose"], a => a.Purpose == "Study",
            _ => Create("study-planning", "Review study and student support steps", "Confirm enrolment requirements and any support available through your education provider.", "Study", 3),
            "The respondent's stated purpose is study."),
        new("work-onboarding", ["purpose"], a => a.Purpose == "Work",
            _ => Create("work-onboarding", "Review work and employment onboarding", "Confirm your employment documents, workplace onboarding, and practical arrangements with your employer.", "Work", 3),
            "The respondent's stated purpose is work."),
        new("family-planning", ["purpose"], a => a.Purpose == "Family",
            _ => Create("family-planning", "Review family settlement needs", "Check which practical registration, childcare, school, or family-support steps apply to your household.", "Family", 3),
            "The respondent's stated purpose is family."),
    ];

    public List<RoadmapTask> Generate(RoadmapRequestDto answers)
    {
        var tasks = Rules.Where(rule => rule.Applies(answers)).Select(rule =>
        {
            var task = rule.CreateTask(answers);
            task.RuleExplanation = rule.Explanation;
            return task;
        }).ToList();
        if (tasks.Select(task => task.RuleId).Distinct(StringComparer.Ordinal).Count() != tasks.Count)
            throw new InvalidOperationException("Roadmap rules produced duplicate task IDs.");
        return RoadmapTaskOrdering.Topological(tasks);
    }
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
            foreach (var prerequisite in task.Prerequisites.Select(p => p.RuleId).Where(tasks.ContainsKey).Order(StringComparer.Ordinal))
                Visit(tasks[prerequisite]);
            visiting.Remove(task.RuleId);
            visited.Add(task.RuleId);
            ordered.Add(task);
        }

        foreach (var task in tasks.Values.OrderBy(t => t.Priority).ThenBy(t => t.RuleId, StringComparer.Ordinal)) Visit(task);
        return ordered;
    }
}

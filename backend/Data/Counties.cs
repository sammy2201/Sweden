namespace SwedenStart;

public static class CountyData
{
    private static readonly Dictionary<string, string[]> CountyAliases = new(StringComparer.Ordinal)
    {
        ["blekinge"] = ["Blekinge"],
        ["dalarna"] = ["Dalarna"],
        ["gavleborg"] = ["Gävleborg", "Gävleborgs"],
        ["gotland"] = ["Gotland"],
        ["halland"] = ["Halland", "Hallands"],
        ["jamtlandharjedalen"] = ["Jämtland", "Jämtlands", "Jämtland Härjedalen"],
        ["jonkoping"] = ["Jönköping", "Jönköpings"],
        ["kalmar"] = ["Kalmar"],
        ["kronoberg"] = ["Kronoberg", "Kronobergs"],
        ["norrbotten"] = ["Norrbotten", "Norrbottens"],
        ["orebro"] = ["Örebro"],
        ["ostergotland"] = ["Östergötland", "Östergötlands"],
        ["skane"] = ["Skåne"],
        ["sormland"] = ["Sörmland", "Södermanland", "Södermanlands"],
        ["stockholm"] = ["Stockholm", "Stockholms"],
        ["uppsala"] = ["Uppsala"],
        ["varmland"] = ["Värmland", "Värmlands"],
        ["vasterbotten"] = ["Västerbotten", "Västerbottens"],
        ["vasternorrland"] = ["Västernorrland", "Västernorrlands"],
        ["vastmanland"] = ["Västmanland", "Västmanlands"],
        ["vastragotaland"] = ["Västra Götaland", "Västra Götalands"]
    };

    private static readonly List<CountyDto> Counties =
    [
        new()
        {
            Id = "blekinge",
            Name = "Blekinge",
            Description = "Known as Sweden's Garden with beautiful coastline and islands.",
            TransportProviderName = "Blekingetrafiken",
            OfficialTicketUrl = "https://www.blekingetrafiken.se/biljetter/",
            JourneyPlannerUrl = "https://www.blekingetrafiken.se/reseinformation/sok-resa/",
        },
        new()
        {
            Id = "dalarna",
            Name = "Dalarna",
            Description = "Traditional Swedish culture, mountains and outdoor adventures.",
            TransportProviderName = "Dalatrafik",
            OfficialTicketUrl = "https://www.dalatrafik.se/sa-reser-du-med-oss/infor-resan/vara-biljetter/",
            JourneyPlannerUrl = "https://www.dalatrafik.se/",
        },
        new()
        {
            Id = "gavleborg",
            Name = "Gävleborg",
            Description = "Home to coastal villages, forests and winter activities.",
            TransportProviderName = "X-trafik",
            OfficialTicketUrl = "https://xtrafik.se/biljetter",
            JourneyPlannerUrl = "https://xtrafik.se/",
        },
        new()
        {
            Id = "gotland",
            Name = "Gotland",
            Description = "Sweden's largest island, famous for medieval Visby and beaches.",
            TransportProviderName = "Gotlands kollektivtrafik",
            OfficialTicketUrl = "https://gotland.se/rg/gotlands-kollektivtrafik/vara-biljetter",
            JourneyPlannerUrl = "https://gotland.se/rg/gotlands-kollektivtrafik",
        },
        new()
        {
            Id = "halland",
            Name = "Halland",
            Description = "Popular for sandy beaches, cycling routes and coastal towns.",
            TransportProviderName = "Hallandstrafiken",
            OfficialTicketUrl = "https://www.hallandstrafiken.se/biljetter-och-priser",
            JourneyPlannerUrl = "https://www.hallandstrafiken.se/res-med-oss/sok-resa/reseplaneraren",
        },
        new()
        {
            Id = "jamtlandharjedalen",
            Name = "Jämtland Härjedalen",
            Description = "A paradise for skiing, hiking and mountain adventures.",
            TransportProviderName = "Länstrafiken Jämtland",
            OfficialTicketUrl = "https://ltr.se/kundservice/60349.biljetter-och-priser.html",
            JourneyPlannerUrl = "https://ltr.se/",
        },
        new()
        {
            Id = "jonkoping",
            Name = "Jönköping",
            Description = "Located by Lake Vättern with hiking, nature and family attractions.",
            TransportProviderName = "Jönköpings Länstrafik",
            OfficialTicketUrl = "https://www.jlt.se/biljetter-och-priser/kop-biljett/",
            JourneyPlannerUrl = "https://www.jlt.se/",
        },
        new()
        {
            Id = "kalmar",
            Name = "Kalmar",
            Description = "Historic castles, coastal scenery and the gateway to Öland.",
            TransportProviderName = "Kalmar länstrafik",
            OfficialTicketUrl = "https://kalmarlanstrafik.se/biljetter-och-priser/kop-biljett/",
            JourneyPlannerUrl = "https://kalmarlanstrafik.se/",
        },
        new()
        {
            Id = "kronoberg",
            Name = "Kronoberg",
            Description = "Forests, lakes and the heart of Sweden's famous glass kingdom.",
            TransportProviderName = "Länstrafiken Kronoberg",
            OfficialTicketUrl = "https://lanstrafikenkron.se/biljetter",
            JourneyPlannerUrl = "https://lanstrafikenkron.se/",
        },
        new()
        {
            Id = "norrbotten",
            Name = "Norrbotten",
            Description = "Home to Lapland, the Northern Lights and Arctic experiences.",
            TransportProviderName = "Länstrafiken Norrbotten",
            OfficialTicketUrl = "https://lanstrafikennorrbotten.se/reseappen",
            JourneyPlannerUrl = "https://lanstrafikennorrbotten.se/",
        },
        new()
        {
            Id = "orebro",
            Name = "Örebro",
            Description = "Historic castles, lakes and family-friendly attractions.",
            TransportProviderName = "Länstrafiken Örebro",
            OfficialTicketUrl = "https://www.lanstrafiken.se/biljetter/",
            JourneyPlannerUrl = "https://www.lanstrafiken.se/",
        },
        new()
        {
            Id = "ostergotland",
            Name = "Östergötland",
            Description = "Rich in history with castles, canals and picturesque countryside.",
            TransportProviderName = "Östgötatrafiken",
            OfficialTicketUrl = "https://www.ostgotatrafiken.se/biljetter/biljetter-och-priser",
            JourneyPlannerUrl = "https://www.ostgotatrafiken.se/",
        },
        new()
        {
            Id = "skane",
            Name = "Skåne",
            Description = "Sweden's southernmost county, known for beaches, castles and vibrant cities.",
            TransportProviderName = "Skånetrafiken",
            OfficialTicketUrl = "https://www.skanetrafiken.se/biljetter/",
            JourneyPlannerUrl = "https://www.skanetrafiken.se/",
        },
        new()
        {
            Id = "sormland",
            Name = "Sörmland",
            Description = "Historic manors, coastal landscapes and charming small towns.",
            TransportProviderName = "Sörmlandstrafiken",
            OfficialTicketUrl = "https://sormlandstrafiken.se/biljetter/kop-biljett/",
            JourneyPlannerUrl = "https://sormlandstrafiken.se/",
        },
        new()
        {
            Id = "stockholm",
            Name = "Stockholm",
            Description = "Home to Sweden's capital with historic districts and world-class museums.",
            TransportProviderName = "SL",
            OfficialTicketUrl = "https://sl.se/biljetter/sa-koper-du-biljetten",
            JourneyPlannerUrl = "https://sl.se/reseplanering",
        },
        new()
        {
            Id = "uppsala",
            Name = "Uppsala",
            Description = "Famous for its cathedral, university and Viking heritage.",
            TransportProviderName = "UL",
            OfficialTicketUrl = "https://www.ul.se/biljetter/sa-har-koper-du-biljett/",
            JourneyPlannerUrl = "https://www.ul.se/",
        },
        new()
        {
            Id = "varmland",
            Name = "Värmland",
            Description = "A county of forests, lakes and literary heritage.",
            TransportProviderName = "Värmlandstrafik",
            OfficialTicketUrl = "https://www.varmlandstrafik.se/varmlandstrafik/biljetter",
            JourneyPlannerUrl = "https://www.varmlandstrafik.se/",
        },
        new()
        {
            Id = "vasterbotten",
            Name = "Västerbotten",
            Description = "Northern forests, rivers and the cultural city of Umeå.",
            TransportProviderName = "Länstrafiken Västerbotten",
            OfficialTicketUrl = "https://www.tabussen.nu/lanstrafiken/biljetter-och-priser/har-koper-du-biljetten/",
            JourneyPlannerUrl = "https://www.tabussen.nu/lanstrafiken",
        },
        new()
        {
            Id = "vasternorrland",
            Name = "Västernorrland",
            Description = "Known for the High Coast and dramatic natural landscapes.",
            TransportProviderName = "Din Tur",
            OfficialTicketUrl = "https://www.dintur.se/biljetter-och-reskort/",
            JourneyPlannerUrl = "https://www.dintur.se/",
        },
        new()
        {
            Id = "vastmanland",
            Name = "Västmanland",
            Description = "Industrial heritage, forests and scenic lakes.",
            TransportProviderName = "VL",
            OfficialTicketUrl = "https://www.vl.se/biljetter/kopa-biljett/",
            JourneyPlannerUrl = "https://www.vl.se/",
        },
        new()
        {
            Id = "vastragotaland",
            Name = "Västra Götaland",
            Description = "A diverse region featuring Gothenburg, archipelagos and nature.",
            TransportProviderName = "Västtrafik",
            OfficialTicketUrl = "https://www.vasttrafik.se/biljetter/",
            JourneyPlannerUrl = "https://www.vasttrafik.se/",
        }
    ];

    public static IEnumerable<CountyDto> GetCounties() => Counties;

    public static bool IsValidCountyId(string countyId)
        => Counties.Any(county => string.Equals(county.Id, countyId, StringComparison.Ordinal));

    public static string ResolveCountyId(string? value)
    {
        var key = Normalize(value);
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        foreach (var (countyId, countyNames) in CountyAliases)
        {
            if (Normalize(countyId) == key || countyNames.Any(name => Normalize(name) == key))
            {
                return countyId;
            }
        }

        return string.Empty;
    }

    public static string[] GetCountyFilterValues(string? value)
    {
        var countyId = ResolveCountyId(value);
        if (string.IsNullOrWhiteSpace(countyId))
        {
            return [value?.Trim() ?? string.Empty];
        }

        var displayName = Counties.First(county => county.Id == countyId).Name;
        return CountyAliases[countyId]
            .SelectMany(name => new[] { name, $"{name} län" })
            .Append(countyId)
            .Append(displayName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var normalized = new string(decomposed
            .Where(character => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());

        return normalized.EndsWith("lan", StringComparison.Ordinal)
            ? normalized[..^3]
            : normalized;
    }
}

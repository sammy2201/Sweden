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
            ImageUrl = "/images/counties/Blekinge.jpeg",
        },
        new()
        {
            Id = "dalarna",
            Name = "Dalarna",
            Description = "Traditional Swedish culture, mountains and outdoor adventures.",
            ImageUrl = "/images/counties/Dalarna.jpeg",
        },
        new()
        {
            Id = "gavleborg",
            Name = "Gävleborg",
            Description = "Home to coastal villages, forests and winter activities.",
            ImageUrl = "/images/counties/G%C3%A4vleborg.jpeg",
        },
        new()
        {
            Id = "gotland",
            Name = "Gotland",
            Description = "Sweden's largest island, famous for medieval Visby and beaches.",
            ImageUrl = "/images/counties/gotland.jpeg",
        },
        new()
        {
            Id = "halland",
            Name = "Halland",
            Description = "Popular for sandy beaches, cycling routes and coastal towns.",
            ImageUrl = "/images/counties/Halland.jpeg",
        },
        new()
        {
            Id = "jamtlandharjedalen",
            Name = "Jämtland Härjedalen",
            Description = "A paradise for skiing, hiking and mountain adventures.",
            ImageUrl = "/images/counties/J%C3%A4mtland%20H%C3%A4rjedalen.jpg",
        },
        new()
        {
            Id = "jonkoping",
            Name = "Jönköping",
            Description = "Located by Lake Vättern with hiking, nature and family attractions.",
            ImageUrl = "/images/counties/J%C3%B6nk%C3%B6ping.jpeg",
        },
        new()
        {
            Id = "kalmar",
            Name = "Kalmar",
            Description = "Historic castles, coastal scenery and the gateway to Öland.",
            ImageUrl = "/images/counties/Kalmar.jpeg",
        },
        new()
        {
            Id = "kronoberg",
            Name = "Kronoberg",
            Description = "Forests, lakes and the heart of Sweden's famous glass kingdom.",
            ImageUrl = "/images/counties/Kronoberg.jpeg",
        },
        new()
        {
            Id = "norrbotten",
            Name = "Norrbotten",
            Description = "Home to Lapland, the Northern Lights and Arctic experiences.",
            ImageUrl = "/images/counties/Norrbotten.jpeg",
        },
        new()
        {
            Id = "orebro",
            Name = "Örebro",
            Description = "Historic castles, lakes and family-friendly attractions.",
            ImageUrl = "/images/counties/%C3%96rebro.jpeg",
        },
        new()
        {
            Id = "ostergotland",
            Name = "Östergötland",
            Description = "Rich in history with castles, canals and picturesque countryside.",
            ImageUrl = "/images/counties/%C3%96sterg%C3%B6tland.jpeg",
        },
        new()
        {
            Id = "skane",
            Name = "Skåne",
            Description = "Sweden's southernmost county, known for beaches, castles and vibrant cities.",
            ImageUrl = "/images/counties/skane.jpeg",
        },
        new()
        {
            Id = "sormland",
            Name = "Sörmland",
            Description = "Historic manors, coastal landscapes and charming small towns.",
            ImageUrl = "/images/counties/S%C3%B6rmland.jpeg",
        },
        new()
        {
            Id = "stockholm",
            Name = "Stockholm",
            Description = "Home to Sweden's capital with historic districts and world-class museums.",
            ImageUrl = "/images/counties/stockholm.jpeg",
        },
        new()
        {
            Id = "uppsala",
            Name = "Uppsala",
            Description = "Famous for its cathedral, university and Viking heritage.",
            ImageUrl = "/images/counties/Uppsala%20.jpeg",
        },
        new()
        {
            Id = "varmland",
            Name = "Värmland",
            Description = "A county of forests, lakes and literary heritage.",
            ImageUrl = "/images/counties/V%C3%A4rmland.jpeg",
        },
        new()
        {
            Id = "vasterbotten",
            Name = "Västerbotten",
            Description = "Northern forests, rivers and the cultural city of Umeå.",
            ImageUrl = "/images/counties/V%C3%A4sterbotten.jpeg",
        },
        new()
        {
            Id = "vasternorrland",
            Name = "Västernorrland",
            Description = "Known for the High Coast and dramatic natural landscapes.",
            ImageUrl = "/images/counties/V%C3%A4sternorrland.jpg",
        },
        new()
        {
            Id = "vastmanland",
            Name = "Västmanland",
            Description = "Industrial heritage, forests and scenic lakes.",
            ImageUrl = "/images/counties/V%C3%A4stmanland.jpeg",
        },
        new()
        {
            Id = "vastragotaland",
            Name = "Västra Götaland",
            Description = "A diverse region featuring Gothenburg, archipelagos and nature.",
            ImageUrl = "/images/counties/V%C3%A4stra%20G%C3%B6taland.jpg",
        }
    ];

    public static IEnumerable<CountyDto> GetCounties() => Counties;

    public static string ResolveCountyId(string? value)
    {
        var key = Normalize(value);
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        foreach (var (id, names) in CountyAliases)
        {
            if (Normalize(id) == key || names.Any(name => Normalize(name) == key))
            {
                return id;
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

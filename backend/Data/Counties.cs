namespace SwedenStart;

public static class CountyData
{
    private static readonly List<CountyDto> Counties =
    [
        new()
        {
            Id = "blekinge",
            Name = "Blekinge",
            Description = "Known as Sweden's Garden with beautiful coastline and islands.",
            ImageUrl = "",
        },
        new()
        {
            Id = "dalarna",
            Name = "Dalarna",
            Description = "Traditional Swedish culture, mountains and outdoor adventures.",
            ImageUrl = "",
        },
        new()
        {
            Id = "gavleborg",
            Name = "Gävleborg",
            Description = "Home to coastal villages, forests and winter activities.",
            ImageUrl = "",
        },
        new()
        {
            Id = "gotland",
            Name = "Gotland",
            Description = "Sweden's largest island, famous for medieval Visby and beaches.",
            ImageUrl = "",
        },
        new()
        {
            Id = "halland",
            Name = "Halland",
            Description = "Popular for sandy beaches, cycling routes and coastal towns.",
            ImageUrl = "",
        },
        new()
        {
            Id = "jamtlandharjedalen",
            Name = "Jämtland Härjedalen",
            Description = "A paradise for skiing, hiking and mountain adventures.",
            ImageUrl = "",
        },
        new()
        {
            Id = "jonkoping",
            Name = "Jönköping",
            Description = "Located by Lake Vättern with hiking, nature and family attractions.",
            ImageUrl = "",
        },
        new()
        {
            Id = "kalmar",
            Name = "Kalmar",
            Description = "Historic castles, coastal scenery and the gateway to Öland.",
            ImageUrl = "",
        },
        new()
        {
            Id = "kronoberg",
            Name = "Kronoberg",
            Description = "Forests, lakes and the heart of Sweden's famous glass kingdom.",
            ImageUrl = "",
        },
        new()
        {
            Id = "norrbotten",
            Name = "Norrbotten",
            Description = "Home to Lapland, the Northern Lights and Arctic experiences.",
            ImageUrl = "",
        },
        new()
        {
            Id = "orebro",
            Name = "Örebro",
            Description = "Historic castles, lakes and family-friendly attractions.",
            ImageUrl = "",
        },
        new()
        {
            Id = "ostergotland",
            Name = "Östergötland",
            Description = "Rich in history with castles, canals and picturesque countryside.",
            ImageUrl = "",
        },
        new()
        {
            Id = "skane",
            Name = "Skåne",
            Description = "Sweden's southernmost county, known for beaches, castles and vibrant cities.",
            ImageUrl = "",
        },
        new()
        {
            Id = "sormland",
            Name = "Sörmland",
            Description = "Historic manors, coastal landscapes and charming small towns.",
            ImageUrl = "",
        },
        new()
        {
            Id = "stockholm",
            Name = "Stockholm",
            Description = "Home to Sweden's capital with historic districts and world-class museums.",
            ImageUrl = "",
        },
        new()
        {
            Id = "uppsala",
            Name = "Uppsala",
            Description = "Famous for its cathedral, university and Viking heritage.",
            ImageUrl = "",
        },
        new()
        {
            Id = "varmland",
            Name = "Värmland",
            Description = "A county of forests, lakes and literary heritage.",
            ImageUrl = "",
        },
        new()
        {
            Id = "vasterbotten",
            Name = "Västerbotten",
            Description = "Northern forests, rivers and the cultural city of Umeå.",
            ImageUrl = "",
        },
        new()
        {
            Id = "vasternorrland",
            Name = "Västernorrland",
            Description = "Known for the High Coast and dramatic natural landscapes.",
            ImageUrl = "",
        },
        new()
        {
            Id = "vastmanland",
            Name = "Västmanland",
            Description = "Industrial heritage, forests and scenic lakes.",
            ImageUrl = "",
        },
        new()
        {
            Id = "vastragotaland",
            Name = "Västra Götaland",
            Description = "A diverse region featuring Gothenburg, archipelagos and nature.",
            ImageUrl = "",
        }
    ];

    public static Task<IEnumerable<CountyDto>> GetCountiesAsync()
    {
        return Task.FromResult<IEnumerable<CountyDto>>(Counties);
    }
}
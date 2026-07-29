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

    public static Task<IEnumerable<CountyDto>> GetCountiesAsync()
    {
        return Task.FromResult<IEnumerable<CountyDto>>(Counties);
    }
}
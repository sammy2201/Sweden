namespace SwedenStart;

using Microsoft.EntityFrameworkCore;


[Index(nameof(ExternalId), IsUnique = true)]
public class Attraction
{
     public Guid Id { get; set; }
     public string ExternalId { get; set; } = string.Empty;
     public string Name { get; set; } = string.Empty;
     public string? DescriptionEn { get; set; }
     public string? DescriptionSv { get; set; }
     public string? Website { get; set; }
     public string? County { get; set; }
     public string? City { get; set; }
     public string? Address { get; set; }
     public string? Category { get; set; }
     public string? ImageUrl { get; set; }
     public double? Latitude { get; set; }
     public double? Longitude { get; set; }
     public DateTime LastSynced { get; set; } = DateTime.UtcNow;
}
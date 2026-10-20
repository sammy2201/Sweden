using System.ComponentModel.DataAnnotations;

namespace SwedenStart;

public class ExploreAttractionsRequestDto
{
     [Required]
     public string County { get; set; } = string.Empty;
     public string Category { get; set; } = string.Empty;

     [Range(1, int.MaxValue)]
     public int Page { get; set; } = 1;

     [Range(1, int.MaxValue)]
     public int PageSize { get; set; } = 20;
}

public class ExploreAttractionsResponseDto
{
     public List<AttractionDto> Attractions { get; set; } = [];
     public int Page { get; set; }
     public int PageSize { get; set; }
     public int TotalCount { get; set; }
     public int TotalPages { get; set; }
     public bool HasNextPage { get; set; }
     public bool HasPreviousPage { get; set; }
}

public class ExploreCountiesResponseDto
{
     public List<CountyDto> Counties { get; set; } = [];
}

public class ExploreAttractionDetailResponseDto
{
     public AttractionDetailDto Attraction { get; set; } = new();
}

public class AttractionDto
{
     public string Id { get; set; } = string.Empty;
     public string Name { get; set; } = string.Empty;
     public string City { get; set; } = string.Empty;
     public string Category { get; set; } = string.Empty;
     public string ImageUrl { get; set; } = string.Empty;

}

public class AttractionDetailDto
{
     public string Id { get; set; } = string.Empty;
     public string Name { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public string Address { get; set; } = string.Empty;
     public string Website { get; set; } = string.Empty;
     public string ImageUrl { get; set; } = string.Empty;
     public string Category { get; set; } = string.Empty;

}

public class CountyDto

{
     public string Id { get; set; } = string.Empty;
     public string Name { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public string TransportProviderName { get; set; } = string.Empty;
     public string OfficialTicketUrl { get; set; } = string.Empty;
     public string? JourneyPlannerUrl { get; set; }

}

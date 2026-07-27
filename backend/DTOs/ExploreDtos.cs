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

public class ExploreAttractionDetailRequestDto
{
     [Required]
     public string Id { get; set; } = string.Empty;
}

public class ExploreAttractionDetailResponseDto
{
     public AttractionDetailDto Attraction { get; set; } = new();
}
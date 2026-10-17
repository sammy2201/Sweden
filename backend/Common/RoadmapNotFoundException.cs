namespace SwedenStart;

public sealed class RoadmapNotFoundException : Exception
{
     public RoadmapNotFoundException(string message)
         : base(message)
     {
     }
}

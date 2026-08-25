namespace SwedenStart;

public sealed class RoadmapConflictException : Exception
{
     public RoadmapConflictException(string message)
         : base(message)
     {
     }
}
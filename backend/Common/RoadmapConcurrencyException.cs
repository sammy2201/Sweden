namespace SwedenStart;

public sealed class RoadmapConcurrencyException : Exception
{
    public RoadmapConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

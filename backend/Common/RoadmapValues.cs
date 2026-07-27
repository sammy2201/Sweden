using System;

namespace SwedenStart;

public static class RoadmapValues
{
    public const string Yes = "Yes";
    public const string No = "No";
    public const string LookingForWork = "Looking for work";
    public const string StillLooking = "Still looking";
    public const string Other = "Other";

    public static bool IsYes(string? value)
        => string.Equals(value, Yes, StringComparison.OrdinalIgnoreCase);

    public static bool IsNo(string? value)
        => string.Equals(value, No, StringComparison.OrdinalIgnoreCase);
}
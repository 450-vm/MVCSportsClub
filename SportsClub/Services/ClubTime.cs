namespace SportsClub.Services;

public static class ClubTime
{
    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/Sofia");
    public static DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
    public static DateOnly Today => DateOnly.FromDateTime(LocalNow);
    public static DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);
    public static string Display(DateTime utc) => ToLocal(utc).ToString("dd MMM yyyy, HH:mm");

    public static bool TryToUtc(DateTime local, out DateTime utc)
    {
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        utc = default;
        // Reject nonexistent and ambiguous daylight-saving times instead of guessing.
        if (Zone.IsInvalidTime(local) || Zone.IsAmbiguousTime(local)) return false;
        utc = TimeZoneInfo.ConvertTimeToUtc(local, Zone);
        return true;
    }
}

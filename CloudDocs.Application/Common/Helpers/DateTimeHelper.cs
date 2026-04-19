
namespace CloudDocs.Application.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime? EnsureUtc(DateTime? date)
        {
            if (!date.HasValue) return null;

            return DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
        }
    }
}

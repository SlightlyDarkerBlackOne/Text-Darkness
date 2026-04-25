using System;

namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Search criteria for music events.
    /// </summary>
    [Serializable]
    public sealed class EventSearchFilter
    {
        public DateTime StartUtc { get; }
        public DateTime EndUtc { get; }
        public string Style { get; }
        public int MaxResults { get; }

        public EventSearchFilter(DateTime startDate, DateTime endDate, string style, int maxResults)
        {
            StartUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Local).ToUniversalTime();
            EndUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1d).AddTicks(-1L), DateTimeKind.Local).ToUniversalTime();
            Style = string.IsNullOrWhiteSpace(style) ? EventCalendarConstants.Search.AllStylesValue : style.Trim();
            MaxResults = maxResults;
        }

        public bool IncludeAllStyles => string.Equals(
            Style,
            EventCalendarConstants.Search.AllStylesValue,
            StringComparison.OrdinalIgnoreCase);
    }
}

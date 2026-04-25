using System.Collections;
namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Imports normalized music events into an external calendar.
    /// </summary>
    public interface ICalendarEventImporter
    {
        IEnumerator ImportAsync(MusicEvent musicEvent, System.Action<EventImportResult> completed);
    }
}

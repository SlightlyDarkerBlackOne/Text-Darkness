using System;
using System.Collections;
using EventsCalendar.Domain;

namespace EventsCalendar.Application
{
    /// <summary>
    /// Imports normalized music events into an external calendar.
    /// </summary>
    public interface ICalendarEventImporter
    {
        IEnumerator ImportAsync(MusicEvent musicEvent, Action<EventImportResult> completed);
    }
}

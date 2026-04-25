using System.Collections;
using System.Collections.Generic;
using EventsCalendar.Domain;

namespace EventsCalendar.Application
{
    /// <summary>
    /// Searches external event sources for music events.
    /// </summary>
    public interface IEventSearchProvider
    {
        IEnumerator SearchEvents(EventSearchFilter filter, System.Action<IReadOnlyList<MusicEvent>> onCompleted, System.Action<string> onFailed);
    }
}

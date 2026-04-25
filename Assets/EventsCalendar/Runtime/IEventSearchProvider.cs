using System.Collections;
using System.Collections.Generic;

namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Searches external event sources for music events.
    /// </summary>
    public interface IEventSearchProvider
    {
        IEnumerator SearchEvents(EventSearchFilter filter, System.Action<IReadOnlyList<MusicEvent>> onCompleted, System.Action<string> onFailed);
    }
}

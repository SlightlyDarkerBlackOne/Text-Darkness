namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Represents one Google Calendar import attempt.
    /// </summary>
    public sealed class EventImportResult
    {
        private EventImportResult(string eventTitle, bool isSuccessful, string message)
        {
            EventTitle = eventTitle ?? string.Empty;
            IsSuccessful = isSuccessful;
            Message = message ?? string.Empty;
        }

        public string EventTitle { get; }

        public bool IsSuccessful { get; }

        public string Message { get; }

        public static EventImportResult Successful(string eventTitle)
        {
            return new EventImportResult(eventTitle, true, string.Empty);
        }

        public static EventImportResult Failed(string message)
        {
            return new EventImportResult(string.Empty, false, message);
        }
    }
}

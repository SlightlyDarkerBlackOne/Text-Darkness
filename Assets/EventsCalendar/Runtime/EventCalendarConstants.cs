namespace EventsCalendar.Runtime
{
    public static class EventCalendarConstants
    {
        public const int DefaultMaxEvents = 20;
        public const int DefaultSearchDays = 30;

        public static class Search
        {
            public const int MinimumResults = 1;
            public const int MaximumResults = 200;
            public const string AllStylesValue = "All";
        }

        public static class Ticketmaster
        {
            public const string DiscoveryEventsEndpoint = "https://app.ticketmaster.com/discovery/v2/events.json";
            public const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

            public static class Query
            {
                public const string ApiKey = "apikey";
                public const string ClassificationName = "classificationName";
                public const string StartDateTime = "startDateTime";
                public const string EndDateTime = "endDateTime";
                public const string Keyword = "keyword";
                public const string Size = "size";
                public const string Sort = "sort";
            }

            public static class Values
            {
                public const string MusicClassification = "music";
                public const string SortByDateAscending = "date,asc";
            }
        }

        public static class GoogleCalendar
        {
            public const string PrimaryCalendarId = "primary";
            public const string InsertEventEndpointFormat = "https://www.googleapis.com/calendar/v3/calendars/{0}/events";
            public const string DateTimeFormat = "o";
        }

        public static class Headers
        {
            public const string Authorization = "Authorization";
            public const string BearerPrefix = "Bearer ";
            public const string ContentType = "Content-Type";
            public const string JsonContentType = "application/json";
        }

        public static class Calendar
        {
            public const double DefaultEventDurationHours = 2d;
            public const string UnknownVenue = "Unknown venue";
            public const string AddressSeparator = ", ";
            public const string ImportedDescriptionHeader = "Imported from Events Calendar.";
            public const string DescriptionLineSeparator = "\n";
        }

        public static class MusicStyles
        {
            public const string All = "All";
            public const string Techno = "Techno";
            public const string Rock = "Rock";
            public const string Pop = "Pop";
            public const string Jazz = "Jazz";
            public const string HipHop = "Hip-Hop";
            public const string Electronic = "Electronic";
            public const string Metal = "Metal";

            public static readonly string[] Options =
            {
                All,
                Techno,
                Rock,
                Pop,
                Jazz,
                HipHop,
                Electronic,
                Metal
            };
        }

        public static class Ui
        {
            public const string CanvasName = "EventsCalendarCanvas";
            public const string PanelName = "EventsCalendarPanel";
            public const string TitleText = "Events Calendar";
            public const string StartDateLabel = "Start date";
            public const string EndDateLabel = "End date";
            public const string StyleLabel = "Music style";
            public const string DatePlaceholder = "yyyy-MM-dd";
            public const string DateFormat = "yyyy-MM-dd";
            public const string ImportButtonName = "ImportEventsButton";
            public const string ImportButtonText = "Import to Google Calendar";
            public const string TextObjectName = "Text";
        }

        public static class Status
        {
            public const string Ready = "Choose period and style, then import.";
            public const string Searching = "Searching events...";
            public const string NoEventsFound = "No events found for selected filters.";
            public const string SearchFailedFormat = "Event search failed: {0}";
            public const string ImportingFormat = "Importing {0} events to Google Calendar...";
            public const string ImportFinishedFormat = "Imported {0} events. Failed: {1}.";
        }

        public static class Validation
        {
            public const string InvalidStartDate = "Start date must use yyyy-MM-dd format.";
            public const string InvalidEndDate = "End date must use yyyy-MM-dd format.";
            public const string EndBeforeStart = "End date must be after start date.";
        }

        public static class Messages
        {
            public const string TicketmasterApiKeyMissing = "Ticketmaster API key is required.";
            public const string GoogleAccessTokenMissing = "Google OAuth access token is required.";
            public const string EventMissing = "Event is required.";
            public const string RequiredUiMissing = "Events Calendar UI references are missing.";
            public const string EventSearchFailedFormat = "Ticketmaster request failed: {0}";
            public const string GoogleImportFailedFormat = "Google Calendar import failed: {0}";
        }

        public static class Url
        {
            public const string QuerySeparator = "&";
        }
    }
}

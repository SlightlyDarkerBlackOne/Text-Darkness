namespace EventsCalendar.Shared
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
            public const string MultipleStylesSeparator = ", ";
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

        public static class LargeLanguageModel
        {
            public const string DefaultChatCompletionsEndpoint = "https://api.openai.com/v1/chat/completions";
            public const string DefaultModel = "gpt-4o-mini";
            public const float DefaultTemperature = 0.2f;
            public const string SystemRole = "system";
            public const string UserRole = "user";
            public const string PromptDateFormat = "yyyy-MM-dd";
            public const string PromptFormat = "Return JSON only. Find up to {0} music events between {1} and {2}. Style filter: {3}. Include title, style, venueName, location, url, startUtc, endUtc, description, ticketPrice, priceIncreaseInfo, performerDescription, interestingFact, googleMapsUrl, and lineup.";
            public const string SystemPrompt = "You are an event discovery service. Return accurate public event data as compact JSON with an events array. Use ISO 8601 UTC date times. Use Not available from provider when a field cannot be verified.";
            public const string MarkdownJsonFenceStart = "```json";
            public const string MarkdownFenceEnd = "```";
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
            public const string NotAvailable = "Not available from provider";
            public const string GoogleMapsSearchUrlFormat = "https://www.google.com/maps/search/?api=1&query={0}";
            public const string TimeRangeFormat = "{0:yyyy-MM-dd HH:mm} - {1:yyyy-MM-dd HH:mm}";
            public const string PriceFormat = "{0:0.##} - {1:0.##} {2}";
            public const string SinglePriceFormat = "{0:0.##} {1}";
            public const string LabelFormat = "{0}: {1}";
            public const string PriceLabel = "Ticket price";
            public const string PriceIncreaseLabel = "Price increase";
            public const string PerformerDescriptionLabel = "DJ / performer description";
            public const string InterestingFactLabel = "Interesting fact";
            public const string GoogleMapsLabel = "Google Maps";
            public const string DurationLabel = "Duration";
            public const string LineupLabel = "Lineup";
            public const string EventUrlLabel = "Event URL";
            public const string StyleLabel = "Style";
            public const string VenueLabel = "Venue";
            public const string SalesWindowFormat = "Sales from {0:yyyy-MM-dd HH:mm} to {1:yyyy-MM-dd HH:mm}";
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

        public static class SearchSources
        {
            public const string Ticketmaster = "Ticketmaster";
            public const string Llm = "LLM";
        }

        public static class Ui
        {
            public const string CanvasName = "EventsCalendarCanvas";
            public const string RootName = "EventsCalendarRoot";
            public const string LandingPanelName = "EventsCalendarLandingPage";
            public const string CalendarPanelName = "EventsCalendarImportPage";
            public const string CardName = "EventsCalendarCard";
            public const string AccentName = "EventsCalendarAccent";
            public const string ShellName = "EventsCalendarShell";
            public const string HeroCardName = "EventsCalendarHeroCard";
            public const string FormCardName = "EventsCalendarFormCard";
            public const string MusicStyleDropdownButtonName = "MusicStyleDropdownButton";
            public const string MusicStyleOptionsPanelName = "MusicStyleOptionsPanel";
            public const string MusicStyleToggleNameFormat = "MusicStyleToggle_{0}";
            public const string PreviewPanelName = "EventPreviewPanel";
            public const string PreviewViewportName = "EventPreviewViewport";
            public const string PreviewContentName = "EventPreviewContent";
            public const string PreviewRowName = "EventPreviewRow";
            public const string RetryFailedButtonName = "RetryFailedImportsButton";
            public const string EventSystemName = "EventsCalendarEventSystem";
            public const string CheckmarkObjectName = "Checkmark";
            public const string HeroEyebrow = "Live music planner";
            public const string LandingTitle = "Find events. Sync nights out.";
            public const string LandingSubtitle = "Discover concerts by style and selected period, then send the essential event details straight to Google Calendar.";
            public const string SubscriptionPrice = "5,99 EUR";
            public const string SubscriptionPeriod = "per month";
            public const string SubscriptionLabel = "Subscription";
            public const string LandingFeatureTitle = "Plan faster";
            public const string LandingFeatureDescription = "Search public live music data, enrich event details, and prepare calendar imports from one focused workflow.";
            public const string PrimaryCallToAction = "Start subscription";
            public const string SecondaryCallToAction = "Open calendar importer";
            public const string CalendarTitle = "Calendar importer";
            public const string CalendarSubtitle = "Pick a date range, select one or more styles, then import matching events to Google Calendar.";
            public const string StartDateLabel = "Start date";
            public const string EndDateLabel = "End date";
            public const string StyleLabel = "Music style";
            public const string SearchSourceLabel = "Search source";
            public const string DatePlaceholder = "yyyy-MM-dd";
            public const string StylePlaceholder = "Select music styles";
            public const string SearchSourcePlaceholder = "Ticketmaster or LLM";
            public const string DateFormat = "yyyy-MM-dd";
            public const string BackButtonName = "BackToLandingButton";
            public const string BackButtonText = "Back";
            public const string StartSubscriptionButtonName = "StartSubscriptionButton";
            public const string OpenCalendarButtonName = "OpenCalendarButton";
            public const string ImportButtonName = "ImportEventsButton";
            public const string SearchButtonText = "Search events";
            public const string ImportButtonText = "Import selected";
            public const string RetryFailedButtonText = "Retry failed";
            public const string PreviewTitle = "Preview events";
            public const string PreviewEmptyText = "Search results will appear here before import.";
            public const string PreviewRowFormat = "{0}\n{1} | {2}";
            public const string DuplicateBadge = "Duplicate";
            public const string ImportedBadge = "Imported";
            public const string FailedBadge = "Failed";
            public const string SelectedCountFormat = "{0} selected";
            public const string TextObjectName = "Text";
            public const string InputTextObjectName = "InputText";
            public const string PlaceholderObjectName = "Placeholder";
        }

        public static class Status
        {
            public const string Ready = "Choose period and style, then import.";
            public const string Searching = "Searching events...";
            public const string PreviewReadyFormat = "Found {0} events. Review and choose what to import.";
            public const string NoEventsSelected = "Select at least one event before importing.";
            public const string NoEventsFound = "No events found for selected filters.";
            public const string SearchFailedFormat = "Event search failed: {0}";
            public const string ImportingFormat = "Importing {0} selected events to Google Calendar...";
            public const string ImportFinishedFormat = "Imported {0} events. Failed: {1}. Skipped duplicates: {2}.";
            public const string RetryReadyFormat = "{0} failed imports are ready to retry.";
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
            public const string LargeLanguageModelApiKeyMissing = "LLM API key is required.";
            public const string GoogleAccessTokenMissing = "Google OAuth access token is required.";
            public const string EventMissing = "Event is required.";
            public const string RequiredUiMissing = "Events Calendar UI references are missing.";
            public const string EventSearchFailedFormat = "Ticketmaster request failed: {0}";
            public const string LargeLanguageModelRequestFailedFormat = "LLM request failed: {0}";
            public const string LargeLanguageModelEmptyResponse = "LLM returned no event data.";
            public const string GoogleImportFailedFormat = "Google Calendar import failed: {0}";
        }

        public static class Url
        {
            public const string QuerySeparator = "&";
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventsCalendar.Runtime
{
    [Serializable]
    public sealed class TicketmasterDiscoveryResponse
    {
        public TicketmasterEmbeddedEvents _embedded;
    }

    [Serializable]
    public sealed class TicketmasterEmbeddedEvents
    {
        public TicketmasterEvent[] events;
    }

    [Serializable]
    public sealed class TicketmasterEvent
    {
        public string id;
        public string name;
        public string url;
        public string info;
        public TicketmasterDates dates;
        public TicketmasterEmbeddedVenues _embedded;
        public TicketmasterClassification[] classifications;
    }

    [Serializable]
    public sealed class TicketmasterDates
    {
        public TicketmasterStartDate start;
    }

    [Serializable]
    public sealed class TicketmasterStartDate
    {
        public string localDate;
        public string localTime;
        public string dateTime;
    }

    [Serializable]
    public sealed class TicketmasterEmbeddedVenues
    {
        public TicketmasterVenue[] venues;
    }

    [Serializable]
    public sealed class TicketmasterVenue
    {
        public string name;
        public TicketmasterCity city;
        public TicketmasterCountry country;
        public TicketmasterAddress address;
    }

    [Serializable]
    public sealed class TicketmasterCity
    {
        public string name;
    }

    [Serializable]
    public sealed class TicketmasterCountry
    {
        public string name;
    }

    [Serializable]
    public sealed class TicketmasterAddress
    {
        public string line1;
    }

    [Serializable]
    public sealed class TicketmasterClassification
    {
        public TicketmasterGenre genre;
        public TicketmasterGenre subGenre;
    }

    [Serializable]
    public sealed class TicketmasterGenre
    {
        public string name;
    }

    [Serializable]
    public sealed class GoogleCalendarEventRequest
    {
        public string summary;
        public string location;
        public string description;
        public GoogleCalendarEventDateTime start;
        public GoogleCalendarEventDateTime end;

        public static GoogleCalendarEventRequest FromMusicEvent(MusicEvent musicEvent, string timeZone)
        {
            return new GoogleCalendarEventRequest
            {
                summary = musicEvent.Title,
                location = musicEvent.Location,
                description = BuildDescription(musicEvent),
                start = GoogleCalendarEventDateTime.FromDateTime(musicEvent.StartUtc, timeZone),
                end = GoogleCalendarEventDateTime.FromDateTime(musicEvent.EndUtc, timeZone)
            };
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        private static string BuildDescription(MusicEvent musicEvent)
        {
            List<string> descriptionParts = new List<string>
            {
                EventCalendarConstants.Calendar.ImportedDescriptionHeader
            };

            AddIfPresent(descriptionParts, musicEvent.Description);
            AddIfPresent(descriptionParts, musicEvent.Url);
            AddIfPresent(descriptionParts, musicEvent.Style);
            AddIfPresent(descriptionParts, musicEvent.VenueName);

            return string.Join(EventCalendarConstants.Calendar.DescriptionLineSeparator, descriptionParts);
        }

        private static void AddIfPresent(List<string> values, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }
    }

    [Serializable]
    public sealed class GoogleCalendarEventDateTime
    {
        public string dateTime;
        public string timeZone;

        public static GoogleCalendarEventDateTime FromDateTime(DateTime value, string timeZone)
        {
            return new GoogleCalendarEventDateTime
            {
                dateTime = value.ToUniversalTime().ToString(EventCalendarConstants.GoogleCalendar.DateTimeFormat),
                timeZone = timeZone
            };
        }
    }
}

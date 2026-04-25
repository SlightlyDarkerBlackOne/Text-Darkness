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
        public string pleaseNote;
        public TicketmasterPromoter promoter;
        public TicketmasterDates dates;
        public TicketmasterSales sales;
        public TicketmasterEmbeddedVenues _embedded;
        public TicketmasterClassification[] classifications;
        public TicketmasterPriceRange[] priceRanges;
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
        public TicketmasterAttraction[] attractions;
    }

    [Serializable]
    public sealed class TicketmasterVenue
    {
        public string name;
        public TicketmasterCity city;
        public TicketmasterCountry country;
        public TicketmasterAddress address;
        public TicketmasterLocation location;
    }

    [Serializable]
    public sealed class TicketmasterLocation
    {
        public string longitude;
        public string latitude;
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
    public sealed class TicketmasterAttraction
    {
        public string name;
        public string url;
        public TicketmasterClassification[] classifications;
    }

    [Serializable]
    public sealed class TicketmasterPriceRange
    {
        public string currency;
        public float min;
        public float max;
    }

    [Serializable]
    public sealed class TicketmasterSales
    {
        [Newtonsoft.Json.JsonProperty("public")]
        public TicketmasterPublicSales publicSales;
    }

    [Serializable]
    public sealed class TicketmasterPromoter
    {
        public string name;
        public string description;
    }

    [Serializable]
    public sealed class TicketmasterPublicSales
    {
        public string startDateTime;
        public string endDateTime;
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

            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.DurationLabel, musicEvent.DurationText);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.PriceLabel, musicEvent.TicketPrice);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.PriceIncreaseLabel, musicEvent.PriceIncreaseInfo);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.LineupLabel, musicEvent.Lineup);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.PerformerDescriptionLabel, musicEvent.PerformerDescription);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.InterestingFactLabel, musicEvent.InterestingFact);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.VenueLabel, musicEvent.VenueName);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.GoogleMapsLabel, musicEvent.GoogleMapsUrl);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.EventUrlLabel, musicEvent.Url);
            AddLabeledValue(descriptionParts, EventCalendarConstants.Calendar.StyleLabel, musicEvent.Style);

            return string.Join(EventCalendarConstants.Calendar.DescriptionLineSeparator, descriptionParts);
        }

        private static void AddLabeledValue(List<string> values, string label, string value)
        {
            string displayValue = string.IsNullOrWhiteSpace(value)
                ? EventCalendarConstants.Calendar.NotAvailable
                : value;
            values.Add(string.Format(EventCalendarConstants.Calendar.LabelFormat, label, displayValue));
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

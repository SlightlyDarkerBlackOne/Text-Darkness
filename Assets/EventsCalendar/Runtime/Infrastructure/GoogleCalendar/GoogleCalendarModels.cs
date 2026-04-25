using System;
using System.Collections.Generic;
using EventsCalendar.Domain;
using EventsCalendar.Shared;
using UnityEngine;

namespace EventsCalendar.Infrastructure.GoogleCalendar
{
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

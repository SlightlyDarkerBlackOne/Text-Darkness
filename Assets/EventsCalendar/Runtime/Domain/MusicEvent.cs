using System;
using EventsCalendar.Shared;

namespace EventsCalendar.Domain
{
    /// <summary>
    /// Event data imported from an external provider and mapped to Google Calendar.
    /// </summary>
    [Serializable]
    public sealed class MusicEvent
    {
        private readonly string m_externalId;
        private readonly string m_title;
        private readonly string m_style;
        private readonly string m_venueName;
        private readonly string m_location;
        private readonly string m_url;
        private readonly DateTime m_startUtc;
        private readonly DateTime m_endUtc;
        private readonly string m_description;
        private readonly string m_ticketPrice;
        private readonly string m_priceIncreaseInfo;
        private readonly string m_performerDescription;
        private readonly string m_interestingFact;
        private readonly string m_googleMapsUrl;
        private readonly string m_lineup;

        public MusicEvent(
            string externalId,
            string title,
            string style,
            string venueName,
            string location,
            string url,
            DateTime startUtc,
            DateTime endUtc,
            string description,
            string ticketPrice,
            string priceIncreaseInfo,
            string performerDescription,
            string interestingFact,
            string googleMapsUrl,
            string lineup)
        {
            m_externalId = externalId;
            m_title = title;
            m_style = style;
            m_venueName = venueName;
            m_location = location;
            m_url = url;
            m_startUtc = startUtc;
            m_endUtc = endUtc;
            m_description = description;
            m_ticketPrice = ticketPrice;
            m_priceIncreaseInfo = priceIncreaseInfo;
            m_performerDescription = performerDescription;
            m_interestingFact = interestingFact;
            m_googleMapsUrl = googleMapsUrl;
            m_lineup = lineup;
        }

        public string ExternalId => m_externalId;
        public string Title => m_title;
        public string Style => m_style;
        public string VenueName => m_venueName;
        public string Location => m_location;
        public string Url => m_url;
        public DateTime StartUtc => m_startUtc;
        public DateTime EndUtc => m_endUtc;
        public string DurationText => string.Format(EventCalendarConstants.Calendar.TimeRangeFormat, m_startUtc.ToLocalTime(), m_endUtc.ToLocalTime());
        public string Description => m_description;
        public string TicketPrice => m_ticketPrice;
        public string PriceIncreaseInfo => m_priceIncreaseInfo;
        public string PerformerDescription => m_performerDescription;
        public string InterestingFact => m_interestingFact;
        public string GoogleMapsUrl => m_googleMapsUrl;
        public string Lineup => m_lineup;
    }
}

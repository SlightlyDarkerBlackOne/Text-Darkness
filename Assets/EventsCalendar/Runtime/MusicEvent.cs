using System;
using UnityEngine;

namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Event data imported from an external provider and mapped to Google Calendar.
    /// </summary>
    [Serializable]
    public sealed class MusicEvent
    {
        [SerializeField] private string m_externalId;
        [SerializeField] private string m_title;
        [SerializeField] private string m_style;
        [SerializeField] private string m_venueName;
        [SerializeField] private string m_location;
        [SerializeField] private string m_url;
        [SerializeField] private DateTime m_startUtc;
        [SerializeField] private DateTime m_endUtc;
        [SerializeField] private string m_description;
        [SerializeField] private string m_ticketPrice;
        [SerializeField] private string m_priceIncreaseInfo;
        [SerializeField] private string m_performerDescription;
        [SerializeField] private string m_interestingFact;
        [SerializeField] private string m_googleMapsUrl;
        [SerializeField] private string m_lineup;

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

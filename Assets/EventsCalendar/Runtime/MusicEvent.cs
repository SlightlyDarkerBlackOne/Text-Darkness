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

        public MusicEvent(
            string externalId,
            string title,
            string style,
            string venueName,
            string location,
            string url,
            DateTime startUtc,
            DateTime endUtc,
            string description)
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
        }

        public string ExternalId => m_externalId;
        public string Title => m_title;
        public string Style => m_style;
        public string VenueName => m_venueName;
        public string Location => m_location;
        public string Url => m_url;
        public DateTime StartUtc => m_startUtc;
        public DateTime EndUtc => m_endUtc;
        public string Description => m_description;
    }
}

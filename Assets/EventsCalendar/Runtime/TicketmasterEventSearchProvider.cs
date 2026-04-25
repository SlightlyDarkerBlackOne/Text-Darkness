using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Searches public music events through Ticketmaster Discovery API.
    /// </summary>
    public sealed class TicketmasterEventSearchProvider : IEventSearchProvider
    {
        private readonly string m_apiKey;

        public TicketmasterEventSearchProvider(string _apiKey)
        {
            m_apiKey = _apiKey;
        }

        public IEnumerator SearchEvents(EventSearchFilter _filter, Action<IReadOnlyList<MusicEvent>> _onCompleted, Action<string> _onFailed)
        {
            if (string.IsNullOrWhiteSpace(m_apiKey))
            {
                _onFailed?.Invoke(EventCalendarConstants.Messages.TicketmasterApiKeyMissing);
                yield break;
            }

            using UnityWebRequest request = UnityWebRequest.Get(BuildSearchUrl(_filter));
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                _onFailed?.Invoke(string.Format(EventCalendarConstants.Messages.EventSearchFailedFormat, request.error));
                yield break;
            }

            TicketmasterDiscoveryResponse response = JsonUtility.FromJson<TicketmasterDiscoveryResponse>(request.downloadHandler.text);
            _onCompleted?.Invoke(MapEvents(response));
        }

        private string BuildSearchUrl(EventSearchFilter _filter)
        {
            List<string> query = new List<string>
            {
                $"{EventCalendarConstants.Ticketmaster.Query.ApiKey}={UnityWebRequest.EscapeURL(m_apiKey)}",
                $"{EventCalendarConstants.Ticketmaster.Query.ClassificationName}={UnityWebRequest.EscapeURL(EventCalendarConstants.Ticketmaster.Values.MusicClassification)}",
                $"{EventCalendarConstants.Ticketmaster.Query.StartDateTime}={UnityWebRequest.EscapeURL(FormatTicketmasterDate(_filter.StartUtc))}",
                $"{EventCalendarConstants.Ticketmaster.Query.EndDateTime}={UnityWebRequest.EscapeURL(FormatTicketmasterDate(_filter.EndUtc))}",
                $"{EventCalendarConstants.Ticketmaster.Query.Size}={Mathf.Clamp(_filter.MaxResults, EventCalendarConstants.Search.MinimumResults, EventCalendarConstants.Search.MaximumResults)}",
                $"{EventCalendarConstants.Ticketmaster.Query.Sort}={UnityWebRequest.EscapeURL(EventCalendarConstants.Ticketmaster.Values.SortByDateAscending)}"
            };

            if (!_filter.IncludeAllStyles && !string.IsNullOrWhiteSpace(_filter.Style))
            {
                query.Add($"{EventCalendarConstants.Ticketmaster.Query.Keyword}={UnityWebRequest.EscapeURL(_filter.Style)}");
            }

            return $"{EventCalendarConstants.Ticketmaster.DiscoveryEventsEndpoint}?{string.Join(EventCalendarConstants.Url.QuerySeparator, query)}";
        }

        private static string FormatTicketmasterDate(DateTime _dateTime)
        {
            return _dateTime.ToUniversalTime().ToString(EventCalendarConstants.Ticketmaster.DateTimeFormat);
        }

        private static IReadOnlyList<MusicEvent> MapEvents(TicketmasterDiscoveryResponse _response)
        {
            List<MusicEvent> events = new List<MusicEvent>();

            if (_response?._embedded?.events == null)
            {
                return events;
            }

            foreach (TicketmasterEvent ticketmasterEvent in _response._embedded.events)
            {
                DateTime startUtc = ParseEventStart(ticketmasterEvent);
                DateTime endUtc = startUtc.AddHours(EventCalendarConstants.Calendar.DefaultEventDurationHours);
                TicketmasterVenue venue = ticketmasterEvent?._embedded?.venues != null && ticketmasterEvent._embedded.venues.Length > 0
                    ? ticketmasterEvent._embedded.venues[0]
                    : null;

                events.Add(new MusicEvent(
                    ticketmasterEvent?.id,
                    ticketmasterEvent?.name,
                    ResolveStyle(ticketmasterEvent),
                    BuildVenueName(venue),
                    BuildAddress(venue),
                    ticketmasterEvent?.url,
                    startUtc,
                    endUtc,
                    ticketmasterEvent?.info));
            }

            return events;
        }

        private static DateTime ParseEventStart(TicketmasterEvent _event)
        {
            string dateTimeValue = _event?.dates?.start?.dateTime;

            if (DateTime.TryParse(dateTimeValue, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime parsedDateTime))
            {
                return parsedDateTime.ToUniversalTime();
            }

            string localDateValue = _event?.dates?.start?.localDate;

            if (DateTime.TryParse(localDateValue, out DateTime parsedDate))
            {
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Local).ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        private static string BuildVenueName(TicketmasterVenue _venue)
        {
            return string.IsNullOrWhiteSpace(_venue?.name)
                ? EventCalendarConstants.Calendar.UnknownVenue
                : _venue.name;
        }

        private static string BuildAddress(TicketmasterVenue _venue)
        {
            if (_venue == null)
            {
                return string.Empty;
            }

            List<string> addressParts = new List<string>();

            AddIfPresent(addressParts, _venue.address?.line1);
            AddIfPresent(addressParts, _venue.city?.name);
            AddIfPresent(addressParts, _venue.country?.name);

            return string.Join(EventCalendarConstants.Calendar.AddressSeparator, addressParts);
        }

        private static string ResolveStyle(TicketmasterEvent _event)
        {
            TicketmasterClassification classification = _event?.classifications != null && _event.classifications.Length > 0
                ? _event.classifications[0]
                : null;

            if (!string.IsNullOrWhiteSpace(classification?.subGenre?.name))
            {
                return classification.subGenre.name;
            }

            return classification?.genre?.name ?? string.Empty;
        }

        private static void AddIfPresent(List<string> _values, string _value)
        {
            if (!string.IsNullOrWhiteSpace(_value))
            {
                _values.Add(_value);
            }
        }
    }
}

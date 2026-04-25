using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Imports music events into a Google Calendar using the Google Calendar REST API.
    /// </summary>
    public sealed class GoogleCalendarEventImporter : ICalendarEventImporter
    {
        private readonly string m_accessToken;
        private readonly string m_calendarId;
        private readonly string m_timeZone;

        public GoogleCalendarEventImporter(string accessToken, string calendarId, string timeZone = null)
        {
            m_accessToken = accessToken;
            m_calendarId = string.IsNullOrWhiteSpace(calendarId) ? EventCalendarConstants.GoogleCalendar.PrimaryCalendarId : calendarId;
            m_timeZone = string.IsNullOrWhiteSpace(timeZone) ? TimeZoneInfo.Local.Id : timeZone;
        }

        public IEnumerator ImportAsync(MusicEvent musicEvent, Action<EventImportResult> completed)
        {
            if (musicEvent == null)
            {
                completed?.Invoke(EventImportResult.Failed(EventCalendarConstants.Messages.EventMissing));
                yield break;
            }

            if (string.IsNullOrWhiteSpace(m_accessToken))
            {
                completed?.Invoke(EventImportResult.Failed(EventCalendarConstants.Messages.GoogleAccessTokenMissing));
                yield break;
            }

            string calendarId = UnityWebRequest.EscapeURL(m_calendarId);
            string endpoint = string.Format(EventCalendarConstants.GoogleCalendar.InsertEventEndpointFormat, calendarId);
            string payload = EventCalendarJsonModels.GoogleCalendarEventRequest.FromMusicEvent(musicEvent, m_timeZone).ToJson();

            using UnityWebRequest request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST);
            byte[] body = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(EventCalendarConstants.Headers.Authorization, EventCalendarConstants.Headers.BearerPrefix + m_accessToken);
            request.SetRequestHeader(EventCalendarConstants.Headers.ContentType, EventCalendarConstants.Headers.JsonContentType);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                completed?.Invoke(EventImportResult.Failed(GetErrorMessage(request)));
                yield break;
            }

            completed?.Invoke(EventImportResult.Successful(musicEvent.Title));
        }

        private static string GetErrorMessage(UnityWebRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.downloadHandler?.text))
            {
                return request.downloadHandler.text;
            }

            return request.error;
        }
    }
}

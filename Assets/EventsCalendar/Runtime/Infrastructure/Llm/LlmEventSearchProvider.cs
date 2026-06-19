using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventsCalendar.Application;
using EventsCalendar.Domain;
using EventsCalendar.Shared;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace EventsCalendar.Infrastructure.Llm
{
    /// <summary>
    /// Searches music events through an OpenAI-compatible LLM chat completions endpoint.
    /// </summary>
    public sealed class LlmEventSearchProvider : IEventSearchProvider
    {
        private readonly string m_apiKey;
        private readonly string m_endpoint;
        private readonly string m_model;

        public LlmEventSearchProvider(string apiKey, string endpoint, string model)
        {
            m_apiKey = apiKey;
            m_endpoint = string.IsNullOrWhiteSpace(endpoint) ? EventCalendarConstants.LargeLanguageModel.DefaultChatCompletionsEndpoint : endpoint;
            m_model = string.IsNullOrWhiteSpace(model) ? EventCalendarConstants.LargeLanguageModel.DefaultModel : model;
        }

        public IEnumerator SearchEvents(EventSearchFilter filter, Action<IReadOnlyList<MusicEvent>> onCompleted, Action<string> onFailed)
        {
            if (string.IsNullOrWhiteSpace(m_apiKey))
            {
                onFailed?.Invoke(EventCalendarConstants.Messages.LargeLanguageModelApiKeyMissing);
                yield break;
            }

            string payload = LlmChatCompletionRequest.Create(m_model, BuildPrompt(filter)).ToJson();
            using UnityWebRequest request = new UnityWebRequest(m_endpoint, UnityWebRequest.kHttpVerbPOST);
            byte[] body = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(EventCalendarConstants.Headers.Authorization, EventCalendarConstants.Headers.BearerPrefix + m_apiKey);
            request.SetRequestHeader(EventCalendarConstants.Headers.ContentType, EventCalendarConstants.Headers.JsonContentType);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFailed?.Invoke(string.Format(EventCalendarConstants.Messages.LargeLanguageModelRequestFailedFormat, GetErrorMessage(request)));
                yield break;
            }

            if (!TryParseEvents(request.downloadHandler.text, out IReadOnlyList<MusicEvent> events, out string parseError))
            {
                onFailed?.Invoke(parseError);
                yield break;
            }

            onCompleted?.Invoke(events);
        }

        private static string BuildPrompt(EventSearchFilter filter)
        {
            string style = filter.IncludeAllStyles ? EventCalendarConstants.Search.AllStylesValue : filter.Style;
            return string.Format(
                EventCalendarConstants.LargeLanguageModel.PromptFormat,
                filter.MaxResults,
                filter.StartUtc.ToLocalTime().ToString(EventCalendarConstants.Ui.DateFormat),
                filter.EndUtc.ToLocalTime().ToString(EventCalendarConstants.Ui.DateFormat),
                style);
        }

        private static bool TryParseEvents(string responseJson, out IReadOnlyList<MusicEvent> events, out string error)
        {
            events = Array.Empty<MusicEvent>();
            error = string.Empty;

            LlmChatCompletionResponse response = JsonConvert.DeserializeObject<LlmChatCompletionResponse>(responseJson);
            string content = response?.choices != null && response.choices.Length > 0
                ? response.choices[0]?.message?.content
                : null;

            if (string.IsNullOrWhiteSpace(content))
            {
                error = EventCalendarConstants.Messages.LargeLanguageModelEmptyResponse;
                return false;
            }

            LlmEventSearchResponse eventSearchResponse = JsonConvert.DeserializeObject<LlmEventSearchResponse>(NormalizeJsonContent(content));
            List<MusicEvent> mappedEvents = new List<MusicEvent>();

            if (eventSearchResponse?.events != null)
            {
                foreach (LlmEventData llmEvent in eventSearchResponse.events)
                {
                    mappedEvents.Add(MapEvent(llmEvent));
                }
            }

            events = mappedEvents;
            return true;
        }

        private static MusicEvent MapEvent(LlmEventData llmEvent)
        {
            DateTime startUtc = ParseUtc(llmEvent?.startUtc, DateTime.UtcNow);
            DateTime endUtc = ParseUtc(llmEvent?.endUtc, startUtc.AddHours(EventCalendarConstants.Calendar.DefaultEventDurationHours));

            return new MusicEvent(
                llmEvent?.externalId,
                llmEvent?.title,
                llmEvent?.style,
                llmEvent?.venueName,
                llmEvent?.location,
                llmEvent?.url,
                startUtc,
                endUtc,
                llmEvent?.description,
                llmEvent?.ticketPrice,
                llmEvent?.priceIncreaseInfo,
                llmEvent?.performerDescription,
                llmEvent?.interestingFact,
                llmEvent?.googleMapsUrl,
                llmEvent?.lineup);
        }

        private static DateTime ParseUtc(string value, DateTime fallback)
        {
            return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime parsedDateTime)
                ? parsedDateTime.ToUniversalTime()
                : fallback;
        }

        private static string NormalizeJsonContent(string value)
        {
            string trimmedValue = value.Trim();
            if (!trimmedValue.StartsWith(EventCalendarConstants.LargeLanguageModel.MarkdownJsonFenceStart, StringComparison.Ordinal))
            {
                return trimmedValue;
            }

            trimmedValue = trimmedValue.Substring(EventCalendarConstants.LargeLanguageModel.MarkdownJsonFenceStart.Length).Trim();
            if (trimmedValue.EndsWith(EventCalendarConstants.LargeLanguageModel.MarkdownFenceEnd, StringComparison.Ordinal))
            {
                trimmedValue = trimmedValue.Substring(0, trimmedValue.Length - EventCalendarConstants.LargeLanguageModel.MarkdownFenceEnd.Length).Trim();
            }

            return trimmedValue;
        }

        private static string GetErrorMessage(UnityWebRequest request)
        {
            return string.IsNullOrWhiteSpace(request.downloadHandler?.text)
                ? request.error
                : request.downloadHandler.text;
        }
    }
}

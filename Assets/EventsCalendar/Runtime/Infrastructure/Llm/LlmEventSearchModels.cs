using System;
using EventsCalendar.Shared;

namespace EventsCalendar.Infrastructure.Llm
{
    [Serializable]
    public sealed class LlmChatCompletionRequest
    {
        public string model;
        public float temperature;
        public LlmChatMessage[] messages;

        public static LlmChatCompletionRequest Create(string model, string prompt)
        {
            return new LlmChatCompletionRequest
            {
                model = model,
                temperature = EventCalendarConstants.LargeLanguageModel.DefaultTemperature,
                messages = new[]
                {
                    new LlmChatMessage(EventCalendarConstants.LargeLanguageModel.SystemRole, EventCalendarConstants.LargeLanguageModel.SystemPrompt),
                    new LlmChatMessage(EventCalendarConstants.LargeLanguageModel.UserRole, prompt)
                }
            };
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public sealed class LlmChatMessage
    {
        public string role;
        public string content;

        public LlmChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public sealed class LlmChatCompletionResponse
    {
        public LlmChatChoice[] choices;
    }

    [Serializable]
    public sealed class LlmChatChoice
    {
        public LlmChatMessage message;
    }

    [Serializable]
    public sealed class LlmEventSearchResponse
    {
        public LlmEventData[] events;
    }

    [Serializable]
    public sealed class LlmEventData
    {
        public string externalId;
        public string title;
        public string style;
        public string venueName;
        public string location;
        public string url;
        public string startUtc;
        public string endUtc;
        public string description;
        public string ticketPrice;
        public string priceIncreaseInfo;
        public string performerDescription;
        public string interestingFact;
        public string googleMapsUrl;
        public string lineup;
    }
}

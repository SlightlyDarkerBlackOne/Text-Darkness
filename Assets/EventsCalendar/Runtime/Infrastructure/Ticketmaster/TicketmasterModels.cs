using System;
using Newtonsoft.Json;

namespace EventsCalendar.Infrastructure.Ticketmaster
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
        public TicketmasterEmbeddedResources _embedded;
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
    public sealed class TicketmasterEmbeddedResources
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
        [JsonProperty("public")]
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
}

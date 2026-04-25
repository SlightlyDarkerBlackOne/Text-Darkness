using System;
using EventsCalendar.Application;
using EventsCalendar.Domain;
using EventsCalendar.Infrastructure.GoogleCalendar;
using EventsCalendar.Shared;
using NUnit.Framework;

public sealed class EventSearchFilterTests
{
    [Test]
    public void Constructor_EmptyStyle_IncludesAllStyles()
    {
        EventSearchFilter filter = new EventSearchFilter(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 30),
            string.Empty,
            10);

        Assert.IsTrue(filter.IncludeAllStyles);
        Assert.AreEqual(EventCalendarConstants.Search.AllStylesValue, filter.Style);
    }

    [Test]
    public void Constructor_EndDate_CoversFullSelectedDay()
    {
        DateTime endDate = new DateTime(2026, 4, 30);

        EventSearchFilter filter = new EventSearchFilter(
            new DateTime(2026, 4, 1),
            endDate,
            EventCalendarConstants.MusicStyles.Rock,
            10);

        DateTime expectedLocalEnd = DateTime.SpecifyKind(endDate.Date.AddDays(1d).AddTicks(-1L), DateTimeKind.Local).ToUniversalTime();
        Assert.AreEqual(expectedLocalEnd, filter.EndUtc);
    }

    [Test]
    public void GoogleCalendarRequest_FromMusicEvent_IncludesRichEventDetails()
    {
        MusicEvent musicEvent = new MusicEvent(
            "external-1",
            "Techno Night Zagreb",
            "Techno",
            "Boogaloo",
            "Ulica grada Vukovara 68, Zagreb, Croatia",
            "https://example.com/event",
            new DateTime(2026, 5, 10, 20, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 10, 23, 0, 0, DateTimeKind.Utc),
            "Underground techno event.",
            "25 - 40 EUR",
            "Sales from 2026-04-01 10:00 to 2026-05-10 18:00",
            "DJ Orion performs a peak-time techno set.",
            "Hosted by Night Lab.",
            "https://www.google.com/maps/search/?api=1&query=Boogaloo",
            "DJ Orion, Ana Nova");

        GoogleCalendarEventRequest request = GoogleCalendarEventRequest.FromMusicEvent(musicEvent, "UTC");

        StringAssert.Contains("Ticket price: 25 - 40 EUR", request.description);
        StringAssert.Contains("Price increase: Sales from 2026-04-01 10:00 to 2026-05-10 18:00", request.description);
        StringAssert.Contains("DJ / performer description: DJ Orion performs a peak-time techno set.", request.description);
        StringAssert.Contains("Interesting fact: Hosted by Night Lab.", request.description);
        StringAssert.Contains("Google Maps: https://www.google.com/maps/search/?api=1&query=Boogaloo", request.description);
        StringAssert.Contains("Duration:", request.description);
        StringAssert.Contains("Lineup: DJ Orion, Ana Nova", request.description);
    }
}

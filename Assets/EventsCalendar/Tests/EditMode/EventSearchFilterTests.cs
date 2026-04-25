using System;
using EventsCalendar.Runtime;
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
}

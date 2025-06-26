using System;

namespace MauiApp1.Services
{
    public interface ICalendarService
    {
        void AddEventToCalendar(string title, string description, string location, DateTime startTime, DateTime endTime, bool allDay);
    }
}

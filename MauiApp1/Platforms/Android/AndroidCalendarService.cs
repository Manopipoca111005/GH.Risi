using Android;
using Android.App;
using Android.Content;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using MauiApp1.Services;
using Microsoft.Maui.ApplicationModel;

namespace MauiApp1
{
    public class AndroidCalendarService : ICalendarService
    {
        public void AddEventToCalendar(string title, string description, string location, DateTime startTime, DateTime endTime, bool allDay)
        {
            if (ContextCompat.CheckSelfPermission(global::Android.App.Application.Context, Manifest.Permission.WriteCalendar) != global::Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity!, new string[]
                {
                    Manifest.Permission.WriteCalendar,
                    Manifest.Permission.ReadCalendar
                }, 0);
                return;
            }

            Intent intent = new Intent(Intent.ActionInsert);
            intent.SetData(CalendarContract.Events.ContentUri);

            intent.PutExtra("title", title);
            intent.PutExtra("description", description);
            intent.PutExtra("eventLocation", location);
            intent.PutExtra("allDay", allDay);

            long beginTime = (long)(startTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
            long endTimeMillis = (long)(endTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

            intent.PutExtra(CalendarContract.ExtraEventBeginTime, beginTime);
            intent.PutExtra(CalendarContract.ExtraEventEndTime, endTimeMillis);

            intent.SetFlags(ActivityFlags.NewTask);

            Platform.CurrentActivity?.StartActivity(intent);
        }
    }
}
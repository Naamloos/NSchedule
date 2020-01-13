using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace NSchedule.Activities
{
    [Activity(Label = "ScheduleActivity")]
    public class ScheduleActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.ScheduleActivity);

            Task.Run(async () =>
            {
                await InitializeScheduleAsync();
            });
        }

        // https://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        private int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private async Task InitializeScheduleAsync()
        {
            await Statics.PreloadCollectionsAsync();

            RunOnUiThread(() =>
            {
                Toast.MakeText(this, "Done preloading", ToastLength.Long).Show();
            });

            var weeknum = GetIso8601WeekOfYear(DateTime.Now);
            var schedule = await Statics.RestHelper.GetScheduleAsync(4, 2019, weeknum, 4489);
            var days = schedule.Appointments.Select(x => x.Start.Date).Distinct();
            var lin = FindViewById<LinearLayout>(Resource.Id.scheduleView);

            foreach (var day in days)
            {
                var scroll = new NestedScrollView(this);
                var layout = new LinearLayout(this);
                layout.Orientation = Orientation.Vertical;
                layout.AddView(new TextView(this) { Text = day.ToString("dd-MM-yyyy") });

                foreach(var ap in schedule.Appointments.Where(x => x.Start.Date == day.Date).OrderBy(x => x.Start))
                {
                    var card = LayoutInflater.From(this).Inflate(Resource.Layout.AppointmentCard, null);
                    card.FindViewById<TextView>(Resource.Id.cardtext).Text = $"{ap.Name}";
                    layout.AddView(card);
                }

                scroll.AddView(layout);
                RunOnUiThread(() =>
                {
                    lin.AddView(scroll);
                });
            }
        }
    }
}
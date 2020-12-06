using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule
{
    public static class Extensions
    {
        public static Page GetCurrentPage(this Shell s)
        {
            return (s?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
        }

        public static Color Random(this Color c)
        {
            var colors = typeof(Color).GetFields().ToList().Where(x => x.IsStatic && x.FieldType == typeof(Color)).ToList();
            Random rng = new Random();
            var n = rng.Next(0, colors.Count());
            var col = colors[n];
            return (Color)col.GetValue(null);
        }

        public static int GetIso8601WeekOfYear(this DateTime time)
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
    }
}

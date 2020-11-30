using NSchedule.Entities;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Plugin.Calendar.Models;

namespace NSchedule.ViewModels
{
    public class ScheduleViewViewModel : BaseViewModel
    {
        public int CurrentYear { get; set; } = 2020;
        public int CurrentMonth { get; set; } = 1;
        public Command<DateTime> DayTapped { get; }
        public List<Scheduleable> Scheduleables { get; set; }
        public EventCollection Events { get; set; } = new EventCollection();

        public ScheduleViewViewModel()
        {
            var today = DateTime.Now;
            CurrentYear = today.Year;
            CurrentMonth = today.Month;
            this.DayTapped = new Command<DateTime>(async day => await LoadNewDayAsync(day));
        }

        public void ForSchedules(params Scheduleable[] s)
        {
            this.Scheduleables = s.ToList();
            if (s.Count() < 2)
                this.Title = $"Schedule for {s[0]}.";
            else
                this.Title = $"Comparing {string.Join(", ", s.Select(x => x.Code))}.";
        }

        public async Task LoadNewDayAsync(DateTime d)
        {
            if (!this.Events.Keys.Contains(d))
            {
                var events = new List<CalendarEvent>();

                foreach (var s in Scheduleables)
                {
                    //var cal = Shell.Current.GetCurrentPage().FindByName<Xamarin.Plugin.Calendar.Controls.Calendar>("Schedule");
                    var orgs = Data.OrganisationalUnits.Where(x => s.Orus.Contains(long.Parse(x.Id)));
                    var color = await Database.GetColorForCodeAsync(s.Code);
                    var schedule = new List<Appointment>();
                    foreach (var o in orgs)
                    {
                        // Find appropriate year object
                        var year = Data.Years.First(x =>
                            x.Oru.ToString() == o.Id &&
                            o.Years.Contains(x.Id) &&
                            x.Start < d &&
                            x.End > d);

                        if (year != null)
                        {
                            // bri 'ish people be like sched juul
                            var sched = await Rest.GetScheduleAsync(long.Parse(o.Id), year.ActualYear, d.GetIso8601WeekOfYear(), s.Id);
                            if (sched != null)
                                schedule.AddRange(sched.Appointments);
                        }
                    }

                    foreach (var app in schedule.Where(x => x.Start.DayOfWeek == d.DayOfWeek))
                    {
                        var totalticks = app.End.Ticks - app.Start.Ticks;
                        float prog = (((float)app.End.Ticks - DateTime.Now.Ticks) / totalticks);

                        if (prog > 1)
                            prog = 1;
                        if (prog < 0)
                            prog = 0;

                        prog = 1 - prog;
                        var rooms = Data.Schedulables.Where(x => x.GetType() == typeof(Room) && app.Attendees.Contains(x.Id));
                        var attendees = Data.Schedulables.Where(x => x.GetType() != typeof(Room) && app.Attendees.Contains(x.Id));

                        events.Add(new CalendarEvent()
                        {
                            Name = $"{app.Name}{(Scheduleables.Count > 1 ? $" ({s.Code})" : "")}",
                            Description = $"{app.Start.ToString("HH:mm")} - {app.End.ToString("HH:mm")}",
                            End = app.End,
                            Start = app.Start,
                            ScheduleColor = color,
                            Progress = prog,
                            Rooms = $"Rooms: {string.Join(", ", rooms.Select(x => x.Code))}",
                            Attendees = $"Attendees: {string.Join(", ", attendees.Select(x => x.Code))}"
                        });
                    }
                }
                events = events.OrderBy(x => x.Start).ToList();

                this.Events.Add(d, events);
            }
        }
    }
}

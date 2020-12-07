using NSchedule.Entities;
using NSchedule.Popups;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public int CurrentDay { get; set; }

        public DateTime Today
        {
            get
            {
                return new DateTime(CurrentYear, CurrentMonth, CurrentDay);
            }
            set
            {
                CurrentDay = value.Day;
                CurrentYear = value.Year;
                CurrentMonth = value.Month;
            }
        }

        public Command<DateTime> DayTapped { get; }
        public Command ShareSchedule { get; }
        public List<DatabaseScheduleable> Scheduleables { get; set; }
        public EventCollection Events { get; set; } = new EventCollection();

        public ScheduleViewViewModel()
        {
            var today = DateTime.Now;
            CurrentYear = today.Year;
            CurrentMonth = today.Month;
            CurrentDay = today.Day;
            this.DayTapped = new Command<DateTime>(async day => await LoadNewDayAsync(day));
        }

        public ScheduleViewViewModel(int day, int month, int year)
        {
            CurrentYear = year;
            CurrentMonth = month;
            CurrentDay = day;
            this.DayTapped = new Command<DateTime>(async mday => await LoadNewDayAsync(mday));
            this.ShareSchedule = new Command(async () => await ShareScheduleAsync());
        }

        public void ForSchedules(params DatabaseScheduleable[] s)
        {
            this.Scheduleables = s.ToList();
            if (s.Count() < 2)
                this.Title = $"Schedule for {s[0]}.";
            else
                this.Title = $"Comparing {string.Join(", ", s.Select(x => x.Code))}.";
        }

        private async Task ShareScheduleAsync()
        {
            StaticMethods.Toast($"Selected date: {this.Today.ToShortDateString()}. Coming soon.");
        }

        public async Task LoadNewDayAsync(DateTime d)
        {
            if (!this.Events.Keys.Contains(d))
            {
                var events = new List<CalendarEvent>();

                foreach (var s in Scheduleables)
                {
                    var schedulable = this.Data.Schedulables.First(x => x.Code == s.Code);
                    //var cal = Shell.Current.GetCurrentPage().FindByName<Xamarin.Plugin.Calendar.Controls.Calendar>("Schedule");
                    var orgs = Data.OrganisationalUnits.Where(x => schedulable.Orus.Contains(long.Parse(x.Id)));
                    var color = await Database.GetColorForCodeAsync(s.Code);
                    var schedule = new List<Appointment>();
                    foreach (var o in orgs)
                    {
                        // Find appropriate year object
                        var year = Data.Years.FirstOrDefault(x =>
                            x.Oru.ToString() == o.Id &&
                            o.Years.Contains(x.Id) &&
                            x.Start < d &&
                            x.End > d);

                        if (year != null)
                        {
                            // bri 'ish people be like sched juul
                            var sched = await Rest.GetScheduleAsync(long.Parse(o.Id), year.ActualYear, d.GetIso8601WeekOfYear(), schedulable.Id);
                            if (sched != null)
                                schedule.AddRange(sched.Appointments);
                        }
                    }

                    foreach (var app in schedule.Where(x => x.Start.DayOfWeek == d.DayOfWeek))
                    {
                        var totalticks = app.End.Ticks - app.Start.Ticks;
                        float prog = (float)(app.End.Ticks - DateTime.Now.Ticks) / totalticks;

                        if (prog > 1)
                            prog = 1;
                        else if (prog < 0)
                            prog = 0;

                        prog = 1 - prog;
                        var rooms = Data.Schedulables.Where(x => x.GetType() == typeof(Room) && app.Attendees.Contains(x.Id));
                        var attendees = Data.Schedulables.Where(x => x.GetType() != typeof(Room) && app.Attendees.Contains(x.Id));
                        var cevent = new CalendarEvent()
                        {
                            Name = $"{app.Name}",
                            ScheduleableCode = s.Code,
                            Times = $"{app.Start.ToString("HH:mm")} - {app.End.ToString("HH:mm")}",
                            End = app.End,
                            Start = app.Start,
                            ScheduleColor = color,
                            Progress = prog,
                            Rooms = string.Join(", ", rooms.Select(x => x.Code)),
                            Attendees = string.Join(", ", attendees.Select(x => x.Code)),
                            Appointment = app,
                            ScheduledFor = this.Scheduleables.Count > 1 ? s.Code : ""
                        };

                        cevent.InfoCommand = new Command(async () =>
                        {
                            var pop = new AppointmentInfo(cevent);

                            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(pop);
                        });

                        if (!string.IsNullOrEmpty(app.Attention))
                        {
                            cevent.Urgent = Color.Red;
                        }

                        events.Add(cevent);
                    }
                }
                events = events.OrderBy(x => x.Start).ToList();

                this.Events.Add(d, events);
            }
        }
    }
}

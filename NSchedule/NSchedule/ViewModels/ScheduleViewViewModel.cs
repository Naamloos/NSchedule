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
        public Scheduleable Scheduleable { get; set; }
        public EventCollection Events { get; set; } = new EventCollection();

        public ScheduleViewViewModel()
        {
            var today = DateTime.Now;
            CurrentYear = today.Year;
            CurrentMonth = today.Month;
            this.DayTapped = new Command<DateTime>(async day => await LoadNewDayAsync(day));
        }

        private async Task LoadNewDayAsync(DateTime d)
        {
            //var cal = Shell.Current.GetCurrentPage().FindByName<Xamarin.Plugin.Calendar.Controls.Calendar>("Schedule");
            var orgs = Data.OrganisationalUnits.Where(x => Scheduleable.Orus.Contains(long.Parse(x.Id)));

            var schedule = new List<Appointment>();
            foreach(var o in orgs)
            {
                var sched = await Rest.GetScheduleAsync(long.Parse(o.Id), d.Year, 49, Scheduleable.Id);
                schedule.AddRange(sched.Appointments);
            }

            var events = new List<CalendarEvent>();
            foreach(var app in schedule)
            {
                events.Add(new CalendarEvent()
                {
                    Name = app.Name,
                    Description = "TODO",
                    End = app.End,
                    Start = app.Start
                });
            }

            this.Events.Add(d, events);
        }
    }
}

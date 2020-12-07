using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Plugin.LocalNotifications;

namespace NSchedule.ViewModels
{
    public class AppointmentInfoViewModel : BaseViewModel
    {
        public CalendarEvent Appointment { get; set; }

        public string Times { get; set; }

        public string Rooms { get; set; }

        public string Teachers { get; set; }

        public string Groups { get; set; }

        public Command ShareMe { get; set; }

        public Command RemindMe { get; set; }

        public AppointmentInfoViewModel(CalendarEvent cal)
        {
            this.Appointment = cal;

            var att = cal.Appointment.Attendees;

            var rooms = Data.Schedulables.Where(x => x.GetType() == typeof(Room) && att.Contains(x.Id)).Cast<Room>();
            var teachers = Data.Schedulables.Where(x => x.GetType() == typeof(Teacher) && att.Contains(x.Id)).Cast<Teacher>();
            var groups = Data.Schedulables.Where(x => x.GetType() == typeof(Group) && att.Contains(x.Id)).Cast<Group>();

            this.Title = cal.Name;
            this.Times = cal.Times;
            this.Rooms = string.Join(", ", rooms.Select(x => x.Code));
            this.Teachers = string.Join(", ", teachers.Select(x => x.Code));
            this.Groups = string.Join(", ", groups.Select(x => x.Code));

            this.ShareMe = new Command(async () =>
            {
                await new ImageHelper().ShareAppointmentImageAsync(cal);
            });
            this.RemindMe = new Command(async () => await AddReminderAsync());
        }

        // TODO: removing reminders, (auto setting reminders, etc?)
        private async Task AddReminderAsync()
        {
            var date = this.Appointment.Start.Subtract(TimeSpan.FromMinutes(15));
            var id = date.GetHashCode();
            var newnot = await Database.AddNotificationAsync(id, $"Appointment in 15 minutes!", $"{Appointment.Name} at {Appointment.Rooms}" +
                $" for {Appointment.ScheduleableCode} in 15 minutes!" +
                $"\n({Appointment.Times})", date.DateTime);
            CrossLocalNotifications.Current.Show(newnot.Title, newnot.Text, newnot.Id, newnot.DateTime);
        }
    }
}

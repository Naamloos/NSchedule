using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace NSchedule.Entities
{
    public class CalendarEvent
    {
        public string Name { get; set; }
        public string Times { get; set; }
        public string Rooms { get; set; }
        public string Attendees { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public Color ScheduleColor { get; set; }
        public float Progress { get; set; }
        public Color Urgent { get; set; } = Color.Transparent;
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Appointment Appointment { get; set; }
        public Command InfoCommand { get; set; }
        public string ScheduleableCode { get; set; }
        public string ScheduledFor { get; set; }
        public bool ShowScheduledFor { get { return !string.IsNullOrEmpty(ScheduledFor); } }
    }
}

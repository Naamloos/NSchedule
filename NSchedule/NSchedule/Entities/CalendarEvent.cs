using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace NSchedule.Entities
{
    public class CalendarEvent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rooms { get; set; }
        public string Attendees { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public Color ScheduleColor { get; set; }
        public float Progress { get; set; }
    }
}

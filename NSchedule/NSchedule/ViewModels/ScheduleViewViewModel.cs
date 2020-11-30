using System;
using System.Collections.Generic;
using System.Text;

namespace NSchedule.ViewModels
{
    public class ScheduleViewViewModel : BaseViewModel
    {
        public int CurrentYear { get; set; } = 2020;
        public int CurrentMonth { get; set; } = 1;

        public ScheduleViewViewModel()
        {
            var today = DateTime.Now;
            CurrentYear = today.Year;
            CurrentMonth = today.Month;
        }
    }
}

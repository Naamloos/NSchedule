using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.ViewModels;
using Plugin.Toast;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NSchedule.Views
{
    public partial class ScheduleViewPage : ContentPage
    {
        public ScheduleViewPage(int day, int month, int year, params Scheduleable[] s)
        {
            InitializeComponent();
            this.BindingContext = new ScheduleViewViewModel(day, month, year);
            var binding = (ScheduleViewViewModel)this.BindingContext;
            binding.ForSchedules(s);
            binding.LoadNewDayAsync(new DateTime(year, month, day)).SafeFireAndForget(false);
        }
    }
}
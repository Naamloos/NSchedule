using Emzi0767.Utilities;
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
        Scheduleable[] s;
        int initday;
        int initmonth;
        int inityear;

        public ScheduleViewPage(int day, int month, int year, params Scheduleable[] s)
        {
            InitializeComponent();
            this.BindingContext = new ScheduleViewViewModel(day, month, year);
            this.s = s;
            this.initday = day;
            this.initmonth = month;
            this.inityear = year;
        }

        protected override async void OnAppearing()
        {
            var binding = (ScheduleViewViewModel)this.BindingContext;
            binding.ForSchedules(s);
            await binding.LoadNewDayAsync(new DateTime(inityear, initmonth, initday));
            base.OnAppearing();
        }
    }
}
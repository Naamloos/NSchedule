﻿using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.Popups;
using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class CompareViewModel : BaseViewModel
    {
        public ObservableCollection<DatabaseScheduleable> Schedules { get { return this.Data.Tracked; } set { } }
        public Command Compare { get; }

        public CompareViewModel()
        {
            Title = "Compare";
            this.Compare = new Command(async () => await CompareAsync());
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        private async Task CompareAsync()
        {
            if(this.Schedules.Count(x => x.Selected) < 2)
            {
                StaticMethods.Toast("Select at least 2 schedules!");
                return;
            }
            var today = DateTime.Now;
            await Shell.Current.GetCurrentPage().Navigation.PushAsync(new ScheduleViewPage(today.Day, today.Month, today.Year, this.Schedules.Where(x => x.Selected).ToArray()));
        }
    }
}
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
    // TODO: Rename RoosterView to something like ScheduleListView
    public class RoosterViewModel : BaseViewModel
    {
        public Command AddItemCommand { get; }
        public ObservableCollection<Scheduleable> Schedules { get { return this.Data.Tracked; } set { } }
        public Command<string> RemoveFromListCommand { get; }
        public Command<string> OpenSchedule { get; }

        public RoosterViewModel()
        {
            Title = "Schedules";
            this.AddItemCommand = new Command(async e => await ViewAddDialogAsync(e));
            this.RemoveFromListCommand = new Command<string>(async s => await RemoveFromListAsync(s));
            this.OpenSchedule = new Command<string>(async s => await ItemSelectedAsync(s));
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        private async Task ItemSelectedAsync(string s)
        {
            StaticMethods.Toast($"Selected {s}.");
            Shell.Current.GetCurrentPage().FindByName<ListView>("SelectedSchedules").SelectedItem = null;
            var today = DateTime.Now;
            await Shell.Current.GetCurrentPage().Navigation.PushAsync(new ScheduleViewPage(today.Day, today.Month, today.Year, Data.Schedulables.First(x => x.Code == s)));
        }

        private async Task RemoveFromListAsync(string item)
        {
            StaticMethods.Toast($"Removed {item}.");
            await this.Data.RemoveTrackedSchedule(item);
        }

        private async Task ViewAddDialogAsync(object e)
        {
            var pop = new ScheduleSelect();

            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(pop);
        }
    }
}
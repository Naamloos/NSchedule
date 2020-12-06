using NSchedule.Entities;
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
    public class ScheduleListViewModel : BaseViewModel
    {
        public Command AddItemCommand { get; }
        public ObservableCollection<DatabaseScheduleable> Schedules { get { return this.Data.Tracked; } set { } }
        public Command<string> OpenSchedule { get; }

        public Command<string> EditItem { get; }

        public ScheduleListViewModel()
        {
            Title = "Schedules";
            this.AddItemCommand = new Command(async e => await ViewAddDialogAsync(e));
            this.OpenSchedule = new Command<string>(async s => await ItemSelectedAsync(s));
            this.EditItem = new Command<string>(async s => await EditItemAsync(s));
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
            await Shell.Current.GetCurrentPage().Navigation.PushAsync(new ScheduleViewPage(today.Day, today.Month, today.Year, this.Schedules.First(x => x.Code == s)));
        }

        private async Task ViewAddDialogAsync(object e)
        {
            var pop = new ScheduleSelect();

            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(pop);
        }

        private async Task EditItemAsync(string item)
        {
            StaticMethods.Toast($"Edit popup for {item}.");

            var pop = new EditSchedule();
            pop.BindingContext = new EditScheduleViewModel(this.Schedules.First(x => x.Code == item));

            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(pop);
        }
    }
}
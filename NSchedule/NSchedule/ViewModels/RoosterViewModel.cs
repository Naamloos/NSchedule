using NSchedule.Popups;
using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class RoosterViewModel : BaseViewModel
    {
        public Command AddItemCommand { get; }
        public RoosterViewModel()
        {
            Title = "Schedules";
            this.AddItemCommand = new Command(async e => await ViewAddDialog(e));
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        private async Task ViewAddDialog(object e)
        {

            CrossToastPopUp.Current.ShowToastMessage("Add dialog works.");

            var pop = new ScheduleSelect();

            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(pop);
        }
    }
}
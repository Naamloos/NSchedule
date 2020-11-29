using NSchedule.Views;
using Plugin.Toast;
using System;
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
            Title = "Roosters";
            this.AddItemCommand = new Command(async e => await ViewAddDialog(e));
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        private async Task ViewAddDialog(object e)
        {
            CrossToastPopUp.Current.ShowToastMessage("Add dialog works.");
            await Task.Yield();
        }
    }
}
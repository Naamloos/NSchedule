using NSchedule.Helpers;
using Plugin.Toast;
using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "Welcome";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://github.com/Naamloos/NSchedule"));
            SampleRequest = new Command(async () =>
            {
                var g = await this.Rest.GetGroupsAsync();
                var sample = string.Join(", ", g.Skip(new Random().Next(0, g.Count() - 5)).Take(5).Select(x => x.Code));
                CrossToastPopUp.Current.ShowToastMessage($"Small test:\nFound {g.Count()} total groups.\n\nRandom sample:\n{sample}", Plugin.Toast.Abstractions.ToastLength.Long);
            });
        }

        public ICommand OpenWebCommand { get; }

        public ICommand SampleRequest { get; }
    }
}
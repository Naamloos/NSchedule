using NSchedule.Helpers;
using NSchedule.Views;
using Plugin.Toast;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NSchedule
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var db = new Database();
            var rest = new RestHelper(db);
            var data = new DataHelper(rest, db);
            DependencyService.RegisterSingleton(db);
            DependencyService.RegisterSingleton(rest);
            DependencyService.RegisterSingleton(data);

            MainPage = new AppShell();
            Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        protected override async void OnStart()
        {
            CrossToastPopUp.Current.ShowToastMessage("One moment, trying to re-authenticate...");
            var reauth = await DependencyService.Get<RestHelper>().CheckAndReconnectSessionAsync();
            if (reauth.Success)
            {
                CrossToastPopUp.Current.ShowToastMessage(reauth.Message);
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
                await DependencyService.Get<DataHelper>().PreloadDataAsync();
            }
            else
            {
                //reauth
                CrossToastPopUp.Current.ShowToastMessage("Welcome! Please log in.\nEither your session expired, your credentials changed or you haven't logged in yet.");
                // enable buttons n shit
                var btn = Shell.Current.GetCurrentPage().FindByName<Button>("login");
                btn.IsVisible = true;
                var spinner = Shell.Current.GetCurrentPage().FindByName<ActivityIndicator>("spinner");
                spinner.IsRunning = false;
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

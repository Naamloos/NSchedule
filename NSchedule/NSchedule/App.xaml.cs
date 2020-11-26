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
            DependencyService.RegisterSingleton(db);
            DependencyService.RegisterSingleton(new RestHelper(db));
            MainPage = new AppShell();
            Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        protected override async void OnStart()
        {
            CrossToastPopUp.Current.ShowToastMessage("One moment...");
            var reauth = await DependencyService.Get<RestHelper>().CheckAndReconnectSessionAsync();
            if (reauth.Success)
            {
                CrossToastPopUp.Current.ShowToastMessage(reauth.Message);
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
            }
            else
            {
                //reauth
                CrossToastPopUp.Current.ShowToastMessage("Welcome! Please log in.\nEither your session expired, your credentials changed or you haven't logged in yet.");
                // enable buttons n shit
                var btn = Shell.Current.GetCurrentPage().FindByName<Button>("login");
                btn.IsVisible = true;
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

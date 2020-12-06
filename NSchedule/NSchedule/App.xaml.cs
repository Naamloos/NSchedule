using Emzi0767.Utilities;
using NSchedule.Helpers;
using NSchedule.ViewModels;
using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NSchedule
{
    public partial class App : Application
    {
        public App(string uri = "")
        {
            InitializeComponent();

            var db = new Database();
            var rest = new RestHelper(db);
            var data = new DataHelper(rest, db);
            DependencyService.RegisterSingleton(new AsyncExecutor());
            DependencyService.RegisterSingleton(db);
            DependencyService.RegisterSingleton(rest);
            DependencyService.RegisterSingleton(data);

            MainPage = new AppShell();
            Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        public void SendToSchedule(string code, int month, int day, int year)
        {
            var dat = DependencyService.Get<DataHelper>();

            dat.RedirectMonth = month;
            dat.RedirectDay = day;
            dat.RedirectYear = year;
            dat.RedirectCode = code;
            dat.RedirectOnLaunch = true;
        }

        protected override async void OnStart()
        {
            CrossToastPopUp.Current.ShowToastMessage("One moment, trying to re-authenticate...");
            var reauth = await DependencyService.Get<RestHelper>().CheckAndReconnectSessionAsync().ConfigureAwait(false);
            if (reauth.Success)
            {
                CrossToastPopUp.Current.ShowToastMessage(reauth.Message);
                await DependencyService.Get<DataHelper>().PreloadDataAsync().ConfigureAwait(false);
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}").ConfigureAwait(false);
                var data = DependencyService.Get<DataHelper>();
                if (data.RedirectOnLaunch)
                {
                    data.RedirectOnLaunch = false;
                    if (data.Schedulables.Any(x => x.Code == data.RedirectCode))
                    {
                        var sched = data.Schedulables.First(x => x.Code == data.RedirectCode);
                        var nav = Shell.Current.Navigation;
                        await nav.PushAsync(
                            new ScheduleViewPage(data.RedirectDay, data.RedirectMonth, data.RedirectYear,
                            data.Schedulables.First(x => x.Code == data.RedirectCode))).ConfigureAwait(false);
                    }
                }
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
                var bind = (LoginViewModel)Shell.Current.GetCurrentPage().BindingContext;
                bind.LoginVisible = true;
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

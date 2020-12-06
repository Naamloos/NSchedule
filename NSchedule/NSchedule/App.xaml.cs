using Emzi0767.Utilities;
using NSchedule.Entities;
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
        private Database _db;
        private DataHelper _data;
        private AsyncExecutor _asyncexecutor;
        private RestHelper _rest;

        public App(string uri = "")
        {
            InitializeComponent();

            this._asyncexecutor = new AsyncExecutor();
            DependencyService.RegisterSingleton(this._asyncexecutor);
            this._db = new Database();
            this._rest = new RestHelper(this._db);
            this._data = new DataHelper(this._rest, this._db);
            DependencyService.RegisterSingleton(this._db);
            DependencyService.RegisterSingleton(this._rest);
            DependencyService.RegisterSingleton(this._data);

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
            await this._db.InitializeAsync().ConfigureAwait(false);
            StaticMethods.Toast("One moment, trying to re-authenticate...");
            var reauth = await DependencyService.Get<RestHelper>().CheckAndReconnectSessionAsync().ConfigureAwait(false);
            if (reauth.Success)
            {
                StaticMethods.Toast(reauth.Message);
                await DependencyService.Get<DataHelper>().PreloadDataAsync().ConfigureAwait(false);
                await StaticMethods.SafeGotoAsync($"//{nameof(AboutPage)}");
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
                            new DatabaseScheduleable() { Code = sched.Code, Color = Color.DarkGray.ToHex() }));
                    }
                }
            }
            else
            {
                //reauth
                StaticMethods.Toast("Welcome! Please log in.\nEither your session expired, your credentials changed or you haven't logged in yet.");
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

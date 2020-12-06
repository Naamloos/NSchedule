using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }
        public string Username { get; set; }
        public string Password { get; set; }

        bool loginvisible = false;
        public bool LoginVisible
        {
            get { return loginvisible; }
            set { SetProperty(ref loginvisible, value); }
        }

        public LoginViewModel()
        {
            LoginCommand = new Command(async e => await OnLoginClicked(e));
        }

        private async Task OnLoginClicked(object obj)
        {
            Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = false;
            var spinner = Shell.Current.GetCurrentPage().FindByName<ActivityIndicator>("spinner");
            spinner.IsRunning = true;
            LoginVisible = false;
            StaticMethods.Toast("Attempting login...");
            if (Username is null || Password is null)
            {
                Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = true;
                spinner.IsRunning = false;
                LoginVisible = true;
                StaticMethods.Toast("Please enter both Username and Password.");
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            var auth = await this.Rest.Authenticate(Username, Password);
            StaticMethods.Toast(auth.Message);

            if (auth.Success)
            {
                await StaticMethods.SafeGotoAsync($"//{nameof(AboutPage)}");
                await this.Data.PreloadDataAsync();
            }
            else
            {
                Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = true;
                spinner.IsRunning = false;
                LoginVisible = true;
            }
        }
    }
}

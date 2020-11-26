using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Collections.Generic;
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

        public LoginViewModel()
        {
            LoginCommand = new Command(async e => await OnLoginClicked(e));
        }

        private async Task OnLoginClicked(object obj)
        {
            Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = false;
            CrossToastPopUp.Current.ShowToastMessage("Attempting login...");
            if(Username is null || Password is null)
            {
                Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = true;
                CrossToastPopUp.Current.ShowToastMessage("Please enter both Username and Password.");
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            var auth = await this.Rest.Authenticate(Username, Password);
            CrossToastPopUp.Current.ShowToastMessage(auth.Message);

            if (auth.Success)
            {
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
            }
            else
            {
                Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = true;
            }
        }
    }
}

﻿using NSchedule.Views;
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
        public bool Waiting { get; set; } = true;

        public LoginViewModel()
        {
            LoginCommand = new Command(async e => await OnLoginClicked(e));
        }

        private async Task OnLoginClicked(object obj)
        {
            Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = false;
            Waiting = true;
            CrossToastPopUp.Current.ShowToastMessage("Attempting login...");
            if (Username is null || Password is null)
            {
                Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = true;
                Waiting = false;
                CrossToastPopUp.Current.ShowToastMessage("Please enter both Username and Password.");
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            var auth = await this.Rest.Authenticate(Username, Password);
            CrossToastPopUp.Current.ShowToastMessage(auth.Message);

            if (auth.Success)
            {
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
                await this.Data.PreloadDataAsync();
            }
            else
            {
                Shell.Current.GetCurrentPage().FindByName<Button>("login").IsVisible = true;
                Waiting = false;
            }
        }
    }
}

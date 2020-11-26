﻿using NSchedule.Helpers;
using NSchedule.ViewModels;
using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace NSchedule
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<Database>().DeleteSettingsAsync();
            DependencyService.Get<RestHelper>().Reset();
            CrossToastPopUp.Current.ShowToastMessage("Logged out and deleted credentials/sessions");
            await Current.GoToAsync("//LoginPage");
            var btn = Shell.Current.GetCurrentPage().FindByName<Button>("login");
            btn.IsVisible = true;
        }
    }
}

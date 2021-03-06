﻿using NSchedule.Helpers;
using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Linq;
using System.Threading.Tasks;
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
            ReloadData = new Command(async () =>
            {
                await this.Data.PreloadDataAsync().ConfigureAwait(false);
                StaticMethods.Toast($"Done refreshing cache.");
            });
        }

        public ICommand OpenWebCommand { get; }

        public ICommand ReloadData { get; }
    }
}
using NSchedule.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class RoosterViewModel : BaseViewModel
    {
        

        public RoosterViewModel()
        {
            Title = "Browse";

        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
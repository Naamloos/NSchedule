using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.ViewModels;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NSchedule.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppointmentInfo : Rg.Plugins.Popup.Pages.PopupPage
    {
        public AppointmentInfo(CalendarEvent app)
        {
            InitializeComponent();
            this.BindingContext = new AppointmentInfoViewModel(app);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}
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
    public partial class EditSchedule : Rg.Plugins.Popup.Pages.PopupPage
    {
        public EditSchedule()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var vm = (EditScheduleViewModel)this.BindingContext;
            vm.ColorCircle = this.ColorWheel;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}
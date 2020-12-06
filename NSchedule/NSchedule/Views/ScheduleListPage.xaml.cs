using NSchedule.Entities;
using NSchedule.ViewModels;
using NSchedule.Views;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NSchedule.Views
{
    public partial class ScheduleListPage : ContentPage
    {
        ScheduleListViewModel _viewModel;

        public ScheduleListPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ScheduleListViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var s = (View)sender;
            s.BackgroundColor = Color.Red;
        }
    }
}
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
    public partial class RoosterPage : ContentPage
    {
        RoosterViewModel _viewModel;

        public RoosterPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new RoosterViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            CrossToastPopUp.Current.ShowToastMessage($"Tapped {((Scheduleable)(e.SelectedItem)).Code}.");
            this.SelectedSchedules.SelectedItem = null;
        }
    }
}
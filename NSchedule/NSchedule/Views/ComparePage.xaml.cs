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
    public partial class ComparePage : ContentPage
    {
        CompareViewModel _viewModel;

        public ComparePage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new CompareViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}
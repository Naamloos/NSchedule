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
    public partial class ScheduleSelect : Rg.Plugins.Popup.Pages.PopupPage
    {
        public ScheduleSelect()
        {
            InitializeComponent();
            this.SearchEntry.TextChanged += (sender, e) =>
            {
                this.Binding.Items.Clear();
                foreach (var s in DependencyService.Get<DataHelper>().Schedulables)
                {
                    if (s.Code.ToLower().Contains(e.NewTextValue.ToLower()))
                        this.Binding.Items.Add(s);
                }
            };

            this.Schedulables.ItemSelected += async (sender, e) =>
            {
                StaticMethods.Toast($"Added schedule for {((Scheduleable)e.SelectedItem).Code}.");
                await DependencyService.Get<DataHelper>().AddTrackedSchedule(((Scheduleable)e.SelectedItem).Code).ConfigureAwait(false);
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
            };
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
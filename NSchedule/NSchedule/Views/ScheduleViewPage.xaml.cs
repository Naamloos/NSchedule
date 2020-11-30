using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.ViewModels;
using Plugin.Toast;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NSchedule.Views
{
    public partial class ScheduleViewPage : ContentPage
    {
        public ScheduleViewPage(params Scheduleable[] s)
        {
            InitializeComponent();
            var binding = (ScheduleViewViewModel)this.BindingContext;
            binding.ForSchedules(s);
            binding.LoadNewDayAsync(DateTime.Now).SafeFireAndForget(false);
        }
    }
}
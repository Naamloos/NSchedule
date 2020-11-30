using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class ScheduleSelectViewModel : BaseViewModel
    {
        public ObservableCollection<Scheduleable> Items { get; }
        public Command Cancel { get; }

        public ScheduleSelectViewModel()
        {
            Items = new ObservableCollection<Scheduleable>();
            foreach (var s in DependencyService.Get<DataHelper>().Schedulables)
            {
                Items.Add(s);
            }

            this.Cancel = new Command(async x =>
            {
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAllAsync();
            });
        }
    }
}

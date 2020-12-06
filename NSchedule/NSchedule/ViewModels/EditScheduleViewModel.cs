using ColorPicker;
using NSchedule.Entities;
using NSchedule.Helpers;
using NSchedule.Popups;
using NSchedule.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule.ViewModels
{
    public class EditScheduleViewModel : BaseViewModel
    {
        public DatabaseScheduleable Item { get; }

        public Color Color { get => Item.ColorObj; set => Item.ColorObj = value; }

        public string CustomName { get => Item.CustomName; set => Item.CustomName = value; }

        public Command Save { get; }

        public Command Remove { get; }

        public ColorWheel ColorCircle;

        public EditScheduleViewModel(DatabaseScheduleable s)
        {
            this.Title = s.Code;
            this.Item = s;
            this.Save = new Command(async () => await SaveAsync());
            this.Remove = new Command(async () => await RemoveAsync());
        }

        private async Task SaveAsync()
        {
            // workaround for broken binding in plugin
            this.Item.ColorObj = ColorCircle.SelectedColor;
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAllAsync();
            await this.Data.UpdateTrackedAsync(Item);
        }

        private async Task RemoveAsync()
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAllAsync();
            await this.Data.RemoveTrackedSchedule(Item.Code);
        }
    }
}

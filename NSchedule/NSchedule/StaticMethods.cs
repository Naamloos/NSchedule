using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NSchedule
{
    public static class StaticMethods
    {
        public static void Toast(string message)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(() => { CrossToastPopUp.Current.ShowToastMessage(message); });
            else
                CrossToastPopUp.Current.ShowToastMessage(message);
        }

        public static async Task SafeGotoAsync(string page)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync(page); });
            else
                await Shell.Current.GoToAsync(page);
        }

        public static void UiThreadSafeExecute(Action a)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(a);
            else
                a.Invoke();
        }

        public static async Task UiThreadSafeExecuteAsync(Task t)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(async () => { await t; });
            else
                await t;
        }
    }
}

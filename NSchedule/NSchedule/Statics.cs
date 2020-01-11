using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NSchedule.Helpers;

namespace NSchedule
{
    internal static class Statics
    {
        public static RestHelper RestHelper;
        private static bool _initialized = false;

        public static void InitializeStatics()
        {
            if (!_initialized)
            {
                RestHelper = new RestHelper();
                _initialized = true;
            }
        }
    }
}
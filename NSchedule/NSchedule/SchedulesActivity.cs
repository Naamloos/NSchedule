using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NSchedule
{
    [Activity(Label = "SchedulesActivity")]
    public class SchedulesActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SchedulesActivity);
            var schedule = Statics.RestHelper.GetScheduleAsync(4, 2019, 2, 4489).GetAwaiter().GetResult();
            var scrollview = FindViewById<ScrollView>(Resource.Id.classes_temp);

            foreach(var appointment in schedule[0]["apps"].Cast<JsonValue>())
            {
                scrollview.AddView(new TextView(scrollview.Context) 
                { 
                    Text = $"{appointment["name"].ToString()}" 
                });
            }
        }
    }
}
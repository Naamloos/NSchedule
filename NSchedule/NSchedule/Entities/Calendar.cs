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
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Calendar
    {
        [JsonProperty("timeUnits")]
        public Dictionary<string, TimeUnit> TimeUnits { get; set; }

        [JsonProperty("days")]
        public Dictionary<string, string> Days { get; set; }

        [JsonProperty("weeks")]
        public Dictionary<string, List<long>> Weeks { get; set; }

        [JsonProperty("periods")]
        public Dictionary<string, long> Periods { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class TimeUnit
    {
        [JsonProperty("dayPart")]
        public long DayPart { get; set; }

        [JsonProperty("start")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("end")]
        public DateTimeOffset End { get; set; }
    }
}
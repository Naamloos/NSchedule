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
    public class Teacher
    {
        [JsonProperty("attTLs")]
        public List<long> AttTLs { get; set; }

        [JsonProperty("attGLs")]
        public List<long> AttGLs { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("orus")]
        public List<long> Locations { get; set; }

        [JsonProperty("tsss", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tsss { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
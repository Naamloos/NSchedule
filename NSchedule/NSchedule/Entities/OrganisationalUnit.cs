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
    public class OrganisationalUnit
    {
        [JsonProperty("yeas")]
        public List<string> Years { get; set; }

        [JsonProperty("timeZone")]
        public string Timezone { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
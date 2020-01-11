using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NSchedule.Entities
{
    class Schedule
    {
        [JsonPropertyName("iPublicationDate")]
        public DateTimeOffset PublicationDate { get; set; }

        [JsonPropertyName("concept")]
        public bool Concept { get; set; }

        [JsonPropertyName("apps")]
        public JsonElement Appointments { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
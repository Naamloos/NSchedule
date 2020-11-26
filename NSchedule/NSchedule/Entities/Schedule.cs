using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Schedule
    {
        [JsonProperty("iPublicationDate")]
        public DateTimeOffset PublicationDate { get; set; }

        [JsonProperty("concept")]
        public bool Concept { get; set; }

        [JsonProperty("apps")]
        public List<Appointment> Appointments { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class Appointment
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("attention")]
        public string Attention { get; set; }

        [JsonProperty("iStart")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("iEnd")]
        public DateTimeOffset End { get; set; }

        [JsonProperty("atts")]
        public List<long> Attendees { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
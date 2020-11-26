using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Room
    {
        [JsonProperty("orus")]
        public List<long> Location { get; set; }

        [JsonProperty("tsss", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tsss { get; set; } // Volgens mij zijn dit de roosters voor het lokaal, de ids iig

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
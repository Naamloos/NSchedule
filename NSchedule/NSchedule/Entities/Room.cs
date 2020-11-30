using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Room : Scheduleable
    {
        [JsonProperty("tsss", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tsss { get; set; } // Volgens mij zijn dit de roosters voor het lokaal, de ids iig
    }
}
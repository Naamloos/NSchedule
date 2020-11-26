using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Group
    {
        [JsonProperty("attDLs")]
        public List<object> AttDLs { get; set; }

        [JsonProperty("orus")]
        public List<long> Orus { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
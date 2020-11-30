using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Teacher : Scheduleable
    {
        [JsonProperty("attTLs")]
        public List<long> AttTLs { get; set; }

        [JsonProperty("attGLs")]
        public List<long> AttGLs { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tsss", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tsss { get; set; }
    }
}
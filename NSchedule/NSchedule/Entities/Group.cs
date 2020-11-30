using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Group : Scheduleable
    {
        [JsonProperty("attDLs")]
        public List<object> AttDLs { get; set; }
    }
}
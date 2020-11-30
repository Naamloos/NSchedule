using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSchedule.Entities
{
    public abstract class Scheduleable
    {
        [JsonProperty("orus")]
        public List<long> Orus { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public bool Selected { get; set; } = false;

        public override string ToString()
        {
            return this.Code;
        }
    }
}

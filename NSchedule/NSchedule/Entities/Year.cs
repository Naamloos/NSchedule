using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace NSchedule.Entities
{
    public class Year
    {
        [JsonProperty("oru")] // the fuck is een Oru nou weer dan (volgens mij Orgisational Unit oftewel Location class)
        public long Oru { get; set; }

        [JsonProperty("year")]
        public long ActualYear { get; set; }

        [JsonProperty("schs")]
        public List<long> Schs { get; set; } // SCHS IS VOLGENS MIJ SCHEDULES.

        // Deze twee zijn altijd null fuck it
        [JsonProperty("deps")]
        public object Deps { get; set; }

        [JsonProperty("avis")]
        public object Avis { get; set; }

        [JsonProperty("periodCount")]
        public long PeriodCount { get; set; }

        [JsonProperty("hasCalendar")]
        public bool HasCalendar { get; set; }

        [JsonProperty("cal")]
        public string Calendar { get; set; }

        [JsonProperty("iStart")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("iEnd")]
        public DateTimeOffset End { get; set; }

        [JsonProperty("iStartOfDay")]
        public DateTimeOffset StartOfDay { get; set; }

        [JsonProperty("iEndOfDay")]
        public DateTimeOffset EndOfDay { get; set; }

        [JsonProperty("firstDayOfWeek")]
        public long FirstDayOfWeek { get; set; }

        [JsonProperty("lastDayOfWeek")]
        public long LastDayOfWeek { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSchedule.Entities
{
    public class DatabaseScheduleable
    {
        [PrimaryKey]
        [Column("code")]
        public string Code { get; set; }

        [Column("color")]
        public string Color { get; set; } = "#00FF00";
    }
}

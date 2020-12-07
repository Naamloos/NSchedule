using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSchedule.Entities
{
    public class DatabaseNotification
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }
    }
}

using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace NSchedule.Entities
{
    public class DatabaseScheduleable
    {
        [PrimaryKey]
        [Column("code")]
        public string Code { get; set; }

        [Column("color")]
        public string Color { get; set; } = "#00FF00";

        [Column("customname")]
        public string CustomName { get; set; } = "";

        [Ignore]
        public string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(CustomName) ? Code : CustomName;
            }
            set 
            {
                this.CustomName = value;
            }
        }

        [Ignore]
        public Color ColorObj
        {
            get => Xamarin.Forms.Color.FromHex(this.Color);
            set => this.Color = value.ToHex();
        }

        [Ignore]
        public bool Selected { get; set; } = false;
    }
}

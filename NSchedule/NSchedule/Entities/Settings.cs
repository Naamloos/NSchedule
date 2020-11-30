using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Essentials;

// This file has some data that is uhh.. confidential?
namespace NSchedule.Entities
{
    public class Settings
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; } = 0;

        [Column("sess")]
        public string CookieString { get; set; } = "";

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("pass")]
        public string Password { get; set; } = "";

        [Column("locale")]
        public string Locale { get; set; } = "en";
    }
}
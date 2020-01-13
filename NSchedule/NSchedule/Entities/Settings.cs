using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Xamarin.Essentials;

// This file has some data that is uhh.. confidential?
namespace NSchedule.Entities
{
    internal class Settings
    {
        [JsonProperty("rememberpassword", NullValueHandling = NullValueHandling.Ignore)]
        public bool RememberPassword = false;

        [JsonProperty("sessioncookie", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionCookie = "";

        [JsonProperty("usercookie", NullValueHandling = NullValueHandling.Ignore)]
        public string UserCookie = "";

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email = "";

        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password = "";

        public void Save()
        {
            SaveAsync().GetAwaiter().GetResult();
        }

        public async Task SaveAsync()
        {
            var json = JsonConvert.SerializeObject(this);
            await SecureStorage.SetAsync("settings", json);
        }

        public static Settings Load()
        {
            return LoadAsync().GetAwaiter().GetResult();
        }

        public static async Task<Settings> LoadAsync()
        {
            var json = await SecureStorage.GetAsync("settings");

            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<Settings>(json);
            }

            return new Settings();
        }
    }
}
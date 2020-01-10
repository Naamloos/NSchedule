using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NSchedule
{
    internal static class Constants
    {
        // Meta
        internal const string USER_AGENT = "NSchedule for Android";

        // Endpoints
        internal const string API_ENDPOINT = "https://sa-nhlstenden.xedule.nl/api";
        internal const string NHL_STENDEN_AUTH_ENDPOINT = "https://sa-nhlstenden.xedule.nl";
        internal const string STENDEN_AUTH_ENDPOINT = "https://sa-nhlstenden.xedule.nl/stenden";
        internal const string SURF_ENDPOINT = "https://engine.surfconext.nl:443/authentication/sp/consume-assertion";
        internal const string ASSERTION_ENDPOINT = "https://sa-nhlstenden.xedule.nl/authentication/sso/assertionservice.aspx";
        internal const string XEDULE_ENDPOINT = "https://sa-nhlstenden.xedule.nl";

        internal const string SESSION_COOKIE_NAME = "ASP.NET_SessionId";
        internal const string USER_COOKIE_NAME = "User";

        // Regexes
        // ha, haha, hahaha, there's two spaces after options lol. 
        //I added a question mark after the second space just in case something changes and this is future proof (for a little bit)
        internal const string AUTH_REGEX = "id=\"options\"  ?method=\"post\" action=\"(.*?)\"";
        internal const string RELAY_STATE_REGEX = "name=\"RelayState\" value=\"(.*?)\"";
        internal const string SAML_RESPONSE_REGEX = "name=\"SAMLResponse\" value=\"(.*?)\"";
    }
}
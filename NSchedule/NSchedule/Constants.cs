using System;
using System.IO;

namespace NSchedule
{
    internal static class Constants
    {
        // Meta
        internal const string USER_AGENT = "NSchedule for Android";
        internal const string SHARED_WITH = "Shared with NSchedule.";
        // TODO misschien user agent veranderen? Deze user agent zegt praktisch gezien "joehoe! ik gebruik een onofficiele app!"

        // Endpoints
        internal const string API_ENDPOINT = "https://sa-nhlstenden.xedule.nl/api";
        internal const string NHL_STENDEN_AUTH_ENDPOINT = "https://sa-nhlstenden.xedule.nl";
        internal const string STENDEN_AUTH_ENDPOINT = "https://sa-nhlstenden.xedule.nl/stenden";
        internal const string SURF_ENDPOINT = "https://engine.surfconext.nl:443/authentication/sp/consume-assertion";
        internal const string ASSERTION_ENDPOINT = "https://sso.xedule.nl/AssertionService.aspx";
        internal const string XEDULE_ENDPOINT = "https://sa-nhlstenden.xedule.nl";

        // Database constants
        public const string DatabaseFilename = "NSchedule.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
    }
}

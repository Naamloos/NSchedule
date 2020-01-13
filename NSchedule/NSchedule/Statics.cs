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
using NSchedule.Entities;
using NSchedule.Helpers;

namespace NSchedule
{
    internal static class Statics
    {
        public static RestHelper RestHelper;

        public static List<Group> Groups;
        public static List<Teacher> Teachers;
        public static List<OrganisationalUnit> OrgUnits;
        public static List<Year> Years;
        public static List<Room> Facilities;


        private static bool _initialized = false;

        public static void InitializeStatics()
        {
            if (!_initialized)
            {
                RestHelper = new RestHelper();

                _initialized = true;
            }
        }

        public static async Task PreloadCollectionsAsync()
        {
            OrgUnits = await RestHelper.GetOrganisationalUnitsAsync();
            Years = await RestHelper.GetYearsAsync();
            Facilities = await RestHelper.GetRoomsAsync();
            Teachers = await RestHelper.GetTeachersAsync();
            Groups = await RestHelper.GetGroupsAsync();
        }
    }
}
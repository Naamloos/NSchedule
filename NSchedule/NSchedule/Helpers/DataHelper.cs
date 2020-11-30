using NSchedule.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSchedule.Helpers
{
    internal class DataHelper
    {
        private RestHelper _rest;
        private Database _db;

        public List<Year> Years { get; private set; } = new List<Year>();
        public List<OrganisationalUnit> OrganisationalUnits { get; private set; } = new List<OrganisationalUnit>();
        public List<Scheduleable> Schedulables { get; private set; } = new List<Scheduleable>();
        public List<Team> Teams { get; private set; } = new List<Team>();

        public DataHelper(RestHelper rest, Database db)
        {
            this._rest = rest;
            this._db = db;
        }

        public async Task PreloadDataAsync()
        {
            // TODO database cache

            this.OrganisationalUnits.Clear();
            this.Years.Clear();
            this.Schedulables.Clear();
            this.Teams.Clear();

            // Keeping order that xedule uses.
            this.OrganisationalUnits = await this._rest.GetOrganisationalUnitsAsync();
            this.Years = await this._rest.GetYearsAsync();
            this.Schedulables.AddRange(await this._rest.GetTeachersAsync());
            this.Schedulables.AddRange(await this._rest.GetRoomsAsync());
            this.Teams = await this._rest.GetTeamsAsync();
            this.Schedulables.AddRange(await this._rest.GetGroupsAsync());
        }
    }
}

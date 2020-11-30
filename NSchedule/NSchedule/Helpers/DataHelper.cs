using NSchedule.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSchedule.Helpers
{
    internal class DataHelper
    {
        private RestHelper _rest;
        private Database _db;

        public ObservableCollection<Year> Years { get; private set; } = new ObservableCollection<Year>();
        public ObservableCollection<OrganisationalUnit> OrganisationalUnits { get; private set; } = new ObservableCollection<OrganisationalUnit>();
        public ObservableCollection<Scheduleable> Schedulables { get; private set; } = new ObservableCollection<Scheduleable>();
        public ObservableCollection<Team> Teams { get; private set; } = new ObservableCollection<Team>();
        public ObservableCollection<Scheduleable> Tracked { get; private set; } = new ObservableCollection<Scheduleable>();

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
            this.OrganisationalUnits = new ObservableCollection<OrganisationalUnit>(await this._rest.GetOrganisationalUnitsAsync());
            this.Years = new ObservableCollection<Year>( await this._rest.GetYearsAsync());

            var teachers = await this._rest.GetTeachersAsync();
            foreach (var t in teachers)
            {
                this.Schedulables.Add(t);
            }

            var rooms = await this._rest.GetRoomsAsync();
            foreach (var r in rooms)
            {
                this.Schedulables.Add(r);
            }

            this.Teams = new ObservableCollection<Team>(await this._rest.GetTeamsAsync());

            var groups = await this._rest.GetGroupsAsync();
            foreach (var g in groups)
            {
                this.Schedulables.Add(g);
            }

            // Reloading tracked schedules from DB
            foreach(var sch in await this._db.GetSchedulesAsync())
            {
                if(this.Schedulables.Any(x => x.Code == sch))
                {
                    this.Tracked.Add(this.Schedulables.First(x => x.Code == sch));
                }
            }
        }

        public async Task AddTrackedSchedule(string code)
        {
            await this._db.AddScheduleAsync(code);
            this.Tracked.Add(this.Schedulables.First(x => x.Code == code));
        }

        public async Task RemoveTrackedSchedule(string code)
        {
            await this._db.RemoveScheduleAsync(code);
            this.Tracked.Remove(this.Schedulables.First(x => x.Code == code));
        }
    }
}

using NSchedule.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSchedule.Helpers
{
    internal class Database
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection InternalDatabase => lazyInitializer.Value;
        static bool initialized = false;

        public Database()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!InternalDatabase.TableMappings.Any(m => m.MappedType.Name == typeof(Settings).Name))
                {
                    await InternalDatabase.CreateTablesAsync(CreateFlags.None, typeof(Settings)).ConfigureAwait(false);
                }
                if (!InternalDatabase.TableMappings.Any(m => m.MappedType.Name == typeof(DatabaseScheduleable).Name))
                {
                    await InternalDatabase.CreateTablesAsync(CreateFlags.None, typeof(DatabaseScheduleable)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }

        public async Task AddScheduleAsync(string code)
        {
            if(await InternalDatabase.Table<DatabaseScheduleable>().Where(x => x.Code == code).CountAsync() < 1)
            {
                await InternalDatabase.InsertAsync(new DatabaseScheduleable() { Code = code });
            }
        }

        public async Task RemoveScheduleAsync(string code)
        {
            if (await InternalDatabase.Table<DatabaseScheduleable>().Where(x => x.Code == code).CountAsync() > 0)
            {
                var rem = await InternalDatabase.Table<DatabaseScheduleable>().DeleteAsync(x => x.Code == code);
            }
        }

        public async Task<List<string>> GetSchedulesAsync()
        {
            var list = new List<string>();
            list.AddRange((await InternalDatabase.Table<DatabaseScheduleable>().Where(x => true).ToListAsync()).Select(x => x.Code));
            return list;
        }

        public async Task<Settings> GetSettingsAsync()
        {
            if (await InternalDatabase.Table<Settings>().CountAsync() > 0)
            {
                // This table can (SHOULD) only have one value.
                return await InternalDatabase.Table<Settings>().FirstAsync();
            }

            return new Settings();
        }

        public async Task SetSettingsAsync(Settings s)
        {
            if (await InternalDatabase.Table<Settings>().CountAsync() > 0)
            {
                // This table can (SHOULD) only have one value.
                await InternalDatabase.UpdateAsync(s);
            }
            else
            {
                await InternalDatabase.InsertAsync(s);
            }
        }

        public async Task DeleteSettingsAsync()
        {
            if (await InternalDatabase.Table<Settings>().CountAsync() > 0)
            {
                // This table can (SHOULD) only have one value.
                await InternalDatabase.Table<Settings>().DeleteAsync(x => true);
            }
        }
    }
}

using Emzi0767.Utilities;
using NSchedule.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

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
        }

        public async Task InitializeAsync()
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
                if (!InternalDatabase.TableMappings.Any(m => m.MappedType.Name == typeof(DatabaseNotification).Name))
                {
                    await InternalDatabase.CreateTablesAsync(CreateFlags.None, typeof(DatabaseNotification)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }

        public async Task<DatabaseNotification> AddNotificationAsync(int id, string title, string description, DateTime when)
        {
            var newn = new DatabaseNotification() { Id = id, Title = title, Text = description, DateTime = when };
            await InternalDatabase.InsertAsync(newn);
            return newn;
        }

        public async Task<List<DatabaseNotification>> GetNotificationsAsync()
        {
            return await InternalDatabase.Table<DatabaseNotification>().Where(x => true).ToListAsync();
        }

        public async Task<DatabaseScheduleable> AddScheduleAsync(string code)
        {
            if(await InternalDatabase.Table<DatabaseScheduleable>().Where(x => x.Code == code).CountAsync() < 1)
            {
                var c = Color.Default.Random();
                var sc = new DatabaseScheduleable() { Code = code, Color = c.ToHex() };
                await InternalDatabase.InsertAsync(sc).ConfigureAwait(false);
                return sc;
            }

            return await InternalDatabase.Table<DatabaseScheduleable>().FirstAsync(x => x.Code == code);
        }

        public async Task<Color> GetColorForCodeAsync(string code)
        {
            if (await InternalDatabase.Table<DatabaseScheduleable>().Where(x => x.Code == code).CountAsync() > 0)
            {
                var s = await InternalDatabase.Table<DatabaseScheduleable>().FirstAsync(x => x.Code == code).ConfigureAwait(false);
                return s.ColorObj;
            }
            else
            {
                return Color.Default;
            }
        }

        public async Task<DatabaseScheduleable> RemoveScheduleAsync(string code)
        {
            if (await InternalDatabase.Table<DatabaseScheduleable>().Where(x => x.Code == code).CountAsync() > 0)
            {
                var remove = await InternalDatabase.Table<DatabaseScheduleable>().FirstAsync(x => x.Code == code);
                var rem = await InternalDatabase.Table<DatabaseScheduleable>().DeleteAsync(x => x.Code == code).ConfigureAwait(false);
                return remove;
            }

            return null;
        }

        public async Task UpdateScheduleAsync(DatabaseScheduleable s)
        {
            if (await InternalDatabase.Table<DatabaseScheduleable>().Where(x => x.Code == s.Code).CountAsync() > 0)
            {
                var updates = await InternalDatabase.UpdateAsync(s);
            }
        }

        public async Task<List<DatabaseScheduleable>> GetSchedulesAsync()
        {
            var list = new List<DatabaseScheduleable>();
            list.AddRange(await InternalDatabase.Table<DatabaseScheduleable>().Where(x => true).ToListAsync().ConfigureAwait(false));
            return list;
        }

        public async Task<Settings> GetSettingsAsync()
        {
            if (await InternalDatabase.Table<Settings>().CountAsync().ConfigureAwait(false) > 0)
            {
                // This table can (SHOULD) only have one value.
                return await InternalDatabase.Table<Settings>().FirstAsync().ConfigureAwait(false);
            }

            return new Settings();
        }

        public async Task SetSettingsAsync(Settings s)
        {
            if (await InternalDatabase.Table<Settings>().CountAsync().ConfigureAwait(false) > 0)
            {
                // This table can (SHOULD) only have one value.
                await InternalDatabase.UpdateAsync(s).ConfigureAwait(false);
            }
            else
            {
                await InternalDatabase.InsertAsync(s).ConfigureAwait(false);
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

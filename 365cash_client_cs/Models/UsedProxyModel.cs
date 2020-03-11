using _365cash_client_cs.Drivers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _365cash_client_cs.Data.ProxySettings;

namespace _365cash_client_cs.Models
{
    public struct UsedProxyEntity
    {
        public string hostname;
        public int cnt_used;
    }

    public class UsedProxyModel
    {
        const int MAX_CNT_USED_ONE_PROXY_ON_TIME = 1;
        const string TABLE_NAME = "used_proxy";

        public void MigrateUp() =>
            App.db.Execute("CREATE TABLE " + TABLE_NAME + " (" +
                        "id integer PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                        "hostname char(63) NOT NULL, " +
                        "expire int NOT NULL, " +
                        "cnt_used int NOT NULL);");

        public long GetNextExipeUnixtime()
            => ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + (App.settings.cache365.expireMinOneIp * 60);

        public void IncCntUsed(string hostname)
        {
            int id = GetIdByParams(hostname);
            if (id == 0)
            {
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@hostname", hostname),
                    new SQLiteParameter("@expire", GetNextExipeUnixtime())
                };
                App.db.Execute("INSERT INTO " + TABLE_NAME + " " +
                            "(hostname, cnt_used, expire) " +
                            "VALUES " +
                            "(@hostname, 1, @expire);", parameters);
            }
            else
            {
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@id", id),
                    new SQLiteParameter("@expire", GetNextExipeUnixtime())
                };
                App.db.Execute("UPDATE " + TABLE_NAME + " " +
                            "SET " +
                            "cnt_used = cnt_used + 1, expire = @expire " +
                            "WHERE id = @id;", parameters);
            }
        }

        public int GetCntUsed(string hostname)
        {
            try
            {
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@hostname", hostname)
                };
                string result = App.db.One("SELECT cnt_used " +
                                        "FROM " + TABLE_NAME + " " +
                                        "WHERE hostname LIKE @hostname " +
                                        "LIMIT 1;", parameters);
                return int.Parse(result);
            }
            catch (Exception) { }

            return 0;
        }

        public int GetIdByParams(string hostname)
        {
            try
            {
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@hostname", hostname),
                };
                string result = App.db.One("SELECT id " +
                                        "FROM " + TABLE_NAME + " " +
                                        "WHERE hostname LIKE @hostname " +
                                        "LIMIT 1;", parameters);
                return int.Parse(result);
            }
            catch (Exception) { }

            return 0;
        }

        public List<UsedProxyEntity> GetAll()
        {
            List<UsedProxyEntity> listUsed = new List<UsedProxyEntity>();

            try
            {
                DbDataReader reader = App.db.All(
                    "SELECT hostname, cnt_used " +
                    "FROM " + TABLE_NAME + ";");
                if (reader == null) return listUsed;
                while (reader.Read())
                {
                    listUsed.Add(new UsedProxyEntity()
                    {
                        hostname = reader.GetValue(0).ToString(),
                        cnt_used = MAX_CNT_USED_ONE_PROXY_ON_TIME
                    });
                }
            }
            catch (Exception) { }

            return listUsed;
        }

        public void RemoveExpired()
        {
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@expire", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds())
            };

            App.db.Execute("DELETE " +
                        "FROM " + TABLE_NAME + " " +
                        "WHERE expire < @expire;", parameters);
        }

        public ProxySettingsItem FindFreeProxy(string[] bannedProxy)
        {
            var busyHosts = GetAll()
                .Where(el => el.cnt_used >= MAX_CNT_USED_ONE_PROXY_ON_TIME)
                .Select(el => el.hostname)
                .ToList();

            foreach (var item in bannedProxy)
                if (!busyHosts.Contains(item))
                    busyHosts.Add(item);

            var listFree = App.settings
                .proxy
                .items
                .Where(el => !busyHosts.Contains(el.ip));

            Random rnd = new Random();
            listFree = listFree.OrderBy(x => rnd.Next());

            if (listFree.Count() < 1) return null;

            foreach (var itemFree in listFree)
            {
                if (!new Cash365().CheckIsBanForever(itemFree))
                    return itemFree;
            }

            return null;
        }
    }
}

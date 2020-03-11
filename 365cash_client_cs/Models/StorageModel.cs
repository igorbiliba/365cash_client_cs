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
    public class StorageModel
    {
        const string TABLE_NAME = "storage";

        public void MigrateUp() =>
            App.db.Execute( @"CREATE TABLE @storage (
                                key char(63) PRIMARY KEY NOT NULL UNIQUE, 
                                value char(255) NOT NULL
                            );".Replace("@storage", TABLE_NAME));

        public void Set(string key, string value)
        {
            try
            {
                string SQL = @"INSERT OR REPLACE INTO @storage (key, value) VALUES (@key, @value);"
                .Replace("@storage", TABLE_NAME);

                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@key",   key),
                    new SQLiteParameter("@value", value)
                };

                App.db.Execute(SQL, parameters);
            }
            catch (Exception) { }
        }

        public string Get(string key)
        {
            List<string> listUsed = new List<string>();

            try
            {
                SQLiteParameter[] parameters = { new SQLiteParameter("@key", key) };

                DbDataReader reader = App.db.All(
                    @"SELECT value
                    FROM @storage
                    WHERE key LIKE @key;".Replace("@storage", TABLE_NAME)
                    , parameters
                );

                reader.Read();
                return reader.GetValue(0).ToString();
            }
            catch (Exception) { }

            return null;
        }
    }
}

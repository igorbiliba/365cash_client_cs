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
    public class UsedAccountModel
    {
        const string TABLE_NAME = "used_account";

        public void MigrateUp()
        {
            try
            {
                App.db.Execute( "CREATE TABLE " + TABLE_NAME + " (" +
                                "id integer PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                "account_name char(63) NOT NULL);");
            }
            catch (Exception) { }

            try
            {
                App.db.Execute("ALTER TABLE " + TABLE_NAME + " ADD cnt integer NOT NULL DEFAULT 1;");
            }
            catch (Exception) { }
        }

        public void SetUsed(string account_name)
        {
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@account_name", account_name)
            };

            if (!IsExists(account_name)) App.db.Execute( "INSERT INTO " + TABLE_NAME + " " +
                                                         "(account_name, cnt) " +
                                                         "VALUES " +
                                                         "(@account_name, 1);", parameters);
            else                         App.db.Execute( "UPDATE " + TABLE_NAME + " " +
                                                         "SET cnt = cnt+1 " +
                                                         "WHERE account_name LIKE @account_name;", parameters);
        }

        public bool IsExists(string account_name)
            => GetAll().Exists(el => el.Key == account_name);

        public List<KeyValuePair<string, int>> GetAll()
        {
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();

            try
            {
                DbDataReader reader = App.db.All(
                    "SELECT account_name, cnt " +
                    "FROM " + TABLE_NAME + " "+
                    "ORDER BY cnt;");

                if (reader == null) return list;

                while (reader.Read())
                    list.Add(new KeyValuePair<string, int>(
                        reader.GetValue(0).ToString(),
                        int.Parse(reader.GetValue(1).ToString())
                    ));
            }
            catch (Exception) { }

            return list;
        }
    }
}

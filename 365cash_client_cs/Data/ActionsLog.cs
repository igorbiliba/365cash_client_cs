using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.Data
{
    public class ActionDataItem
    {
        public ActionDataItem(string phone, string message)
        {
            this.phone = phone;
            this.message = message;
            this.created_at = DateTime.Now.ToString();
        }

        public string phone;
        public string message;
        public string created_at;
    }

    public class ActionsLog
    {
        public ActionsLog()
        {
            try
            {
                if (!File.Exists(FILE))
                    File.Create(FILE);
            }
            catch (Exception) { }
        }

        string FILE
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ActionsLog.json";
            }
        }

        public void Append(ActionDataItem bullet)
        {
            try
            {
                File.AppendAllText(FILE, JsonConvert.SerializeObject(bullet) + Environment.NewLine);
            }
            catch (Exception) { }
        }
    }
}

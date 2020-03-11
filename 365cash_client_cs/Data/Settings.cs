using Leaf.xNet;
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
    public class ProxySettings
    {
        public class ProxySettingsItem
        {
            public int    port { get; set; }
            public string ip { get; set; }
            public string username { get; set; }
            public string password { get; set; }

            public HttpProxyClient CreateProxyClient()
            {
                //Строка вида - протокол://хост:порт:имя_пользователя:пароль
                string proxyStr = String.Format(
                    "{0}:{1}:{2}:{3}",
                    ip,
                    port,
                    username,
                    password
                );

                return HttpProxyClient.Parse(proxyStr);
            }
        }

        public ProxySettingsItem[] items;

        string getFileName() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ProxySettings.json";

        public bool CreateIfNotExists()
        {
            if (File.Exists(getFileName()))
                return false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(getFileName(), true))
            {
                file.WriteLine(JsonConvert.SerializeObject(this.items));
            }

            return true;
        }

        public void LoadSettings()
        {
            string jsonString = System.IO.File.ReadAllText(getFileName());            
            this.items = JsonConvert.DeserializeObject<ProxySettingsItem[]>(jsonString);
        }

        public void SaveItemsToFile()
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(getFileName(), false))
                {
                    file.WriteLine(
                        JsonConvert.SerializeObject(
                            items
                        )
                    );
                }
            } catch(Exception ex) { }
        }

        public void Remove(string host, int port)
        {
            List<ProxySettingsItem> list = new List<ProxySettingsItem>();

            foreach (var item in items)
            {
                if (item.ip.Trim().ToLower() == host.Trim().ToLower() && item.port == port)
                    continue;

                list.Add(item);
            }

            items = list.ToArray();
            SaveItemsToFile();
        }
    }

    public class Account
    {
        public string login;
        public string passwrd;
    }
    public class Cache365Settings
    {
        string FILE_NAME
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Settings.json"; }
        }

        public Account[]    accounts;
        public int          delay_after_login_from;
        public int          delay_after_login_to;
        public int          delay_before_each_step_from;
        public int          delay_before_each_step_to;
        public int          delay_before_get_qiwi_number_from;
        public int          delay_before_get_qiwi_number_to;
        public int          expireMinOneIp;
        public int          maxHoursTestPeriodProxy = -1;
        public int          avg_time_create_sec;
        public string[]     allowEmails;
        public int          max_time_live;

        public void CreateIfNotExists()
        {
            if (File.Exists(FILE_NAME))
                return;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(FILE_NAME, true))
            {
                file.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this));
            }
        }

        public void LoadSettings()
        {
            string jsonString = System.IO.File.ReadAllText(FILE_NAME);
            Cache365Settings data = Newtonsoft.Json.JsonConvert.DeserializeObject<Cache365Settings>(jsonString);

            this.accounts                          = data.accounts;
            this.delay_after_login_from            = data.delay_after_login_from;
            this.delay_after_login_to              = data.delay_after_login_to;
            this.delay_before_each_step_from       = data.delay_before_each_step_from;
            this.delay_before_each_step_to         = data.delay_before_each_step_to;
            this.delay_before_get_qiwi_number_from = data.delay_before_get_qiwi_number_from;
            this.delay_before_get_qiwi_number_to   = data.delay_before_get_qiwi_number_to;
            this.expireMinOneIp                    = data.expireMinOneIp;
            this.maxHoursTestPeriodProxy           = data.maxHoursTestPeriodProxy;
            this.avg_time_create_sec               = data.avg_time_create_sec;
            this.allowEmails                       = data.allowEmails;
            this.max_time_live                     = data.max_time_live;
        }
    }

    public class Settings
    {
        public Cache365Settings cache365
        {
            get
            {
                Cache365Settings settings = new Cache365Settings();
                settings.CreateIfNotExists();
                settings.LoadSettings();
                return settings;
            }
        }

        public ProxySettings proxy
        {
            get
            {
                ProxySettings settings = new ProxySettings();
                if (!settings.CreateIfNotExists())
                    settings.LoadSettings();

                return settings;
            }
        }
    }
}

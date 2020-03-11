using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _365cash_client_cs.Data
{
    public class ProxyLogItem
    {
        public string Host;
        public DateTime LastSuccessDateTime = new DateTime(1999, 1, 1);
        public DateTime LastUsedDateTime = new DateTime(1999, 1, 1);
        public bool InBlackList = false;
    }

    public class ProxyLog
    {
        public ProxyLogItem[] list = new ProxyLogItem[0];

        string FILE
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ProxyLog.json";
            }
        }

        public ProxyLog UpdateIsInBlackList(int expireH)
        {
            for (int i = 0; i < list.Length; i++)
            {
                //если разница меджу удачным использованием и неудачным больше n часов из настроек
                list[i].InBlackList = list[i].LastSuccessDateTime.AddHours(expireH) <= list[i].LastUsedDateTime;
            }

            return this;
        }

        public ProxyLog Load()
        {
            if (!File.Exists(FILE))
                return this;

            list = JsonConvert
                .DeserializeObject<ProxyLogItem[]>(
                    File.ReadAllText(
                        FILE
                    )
                );

            return this;
        }

        ProxyLogItem Get(string host)
        {
            return list.Where(
                el => el.Host.Trim().ToLower() == host.Trim().ToLower()
            )
            .Last();
        }

        void Set(ProxyLogItem item)
        {
            var items = list.ToList();

            var old = items.FindIndex(el => el.Host == item.Host);
            if (old > -1)
            {
                if (item.LastUsedDateTime.Year > 2000)
                    items[old].LastUsedDateTime = item.LastUsedDateTime;

                if (item.LastSuccessDateTime.Year > 2000)
                    items[old].LastSuccessDateTime = item.LastSuccessDateTime;
            }
            else
            {
                items.Add(item);
            }

            list = items.ToArray();
        }

        void TrySave()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(FILE, false))
            {
                file.WriteLine(
                    JsonConvert.SerializeObject(
                        this.list
                    )
                );
            }
        }

        public ProxyLog Save()
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].LastUsedDateTime.Year < 2000)
                    list[i].LastUsedDateTime = list[i].LastSuccessDateTime;

                if (list[i].LastSuccessDateTime.Year < 2000)
                    list[i].LastSuccessDateTime = list[i].LastUsedDateTime;
            }

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    TrySave();
                    continue;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }

            return this;
        }

        public ProxyLog Success(string host, DateTime now)
        {
            Set(new ProxyLogItem()
            {
                Host = host,
                LastSuccessDateTime = now
            });

            return this;
        }

        public ProxyLog Fail(string host, DateTime now)
        {

            Set(new ProxyLogItem()
            {
                Host = host,
                LastUsedDateTime = now
            });

            return this;
        }

        public string[] GetBlacklistHosts()
        {
            return
                this
                .list
                .Where(el => el.InBlackList)
                .Select(el => el.Host)
                .ToArray();
        }
    }
}

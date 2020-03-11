using _365cash_client_cs.Components;
using _365cash_client_cs.Data;
using _365cash_client_cs.Drivers;
using _365cash_client_cs.Parsers;
using _365cash_client_cs.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static _365cash_client_cs.Data.ProxySettings;
using static _365cash_client_cs.Drivers.Cash365;

namespace _365cash_client_cs
{
    class Program
    {
        const int ACTION_ID = 0;

        static void Main(string[] args)
        {
            //args = new string[] {
            //    "--create",
            //    "6000",
            //    "+79060671243",
            //    "3J6jjLs8DBpqPZvNoohDzzsRUqzgWyeMfG"
            //};


            int cntTry = 60;
            while(--cntTry > 0)
            {
                try
                {
                    App.Init();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            if(App.settings.cache365.max_time_live > 0)
                Task.Delay(App.settings.cache365.max_time_live * 1000).ContinueWith(t => {
                    System.Environment.Exit(-1);
                });

            try
            {
                new ProxyLog()
                    .Load()
                    .UpdateIsInBlackList(App.settings.cache365.maxHoursTestPeriodProxy)
                    .Save();
            }
            catch (Exception) { }

            if (args.Length == 0)
            {
                CheckAllProxy();
                Console.ReadKey();
                return;
            }

            try
            {
                switch (args[ACTION_ID])
                {
                    case "--create":
                        double amount = -1;

                        try
                        { amount = double.Parse(args[1].Replace(',', '.')); }
                        catch (Exception)
                        { amount = double.Parse(args[1].Replace('.', ',')); }

                        string btcAddr = args[3];

                        string phone = "+" + (args[2]
                                                .Replace(" ", String.Empty)
                                                .Replace("+", String.Empty)
                                                .Replace("-", String.Empty));

                        ProxySettingsItem proxy = App.usedProxyModel.FindFreeProxy(new string[] { });

                        App.actionsLog.Append(new ActionDataItem(phone, "FindFreeProxy: " + proxy.ip));

                        try
                        {
                            var response = Create(amount, phone, btcAddr, proxy);
                            Console.Write(response.toJson());                            
                        }
                        catch (Exception) {
                            if(proxy != null)
                                App.settings.proxy.Remove(proxy.ip, proxy.port);
                        }                        
                        return;
                    case "--rate":
                        Console.Write(Rate());
                        return;
                    case "--checkallproxy":
                        CheckAllProxy();
                        return;
                }
            }
            catch (Exception) { }
        }

        static void CheckAllProxy()
        {
            var proxyList = App
                .settings
                .proxy
                .items;

            List<ProxySettingsItem> bannedList = new List<ProxySettingsItem>();

            foreach (var proxyItem in proxyList)
            {
                bool isBan = new Cash365()
                    .CheckIsBanForever(proxyItem);

                if (isBan)
                {
                    Console.WriteLine("Ban forever: " + proxyItem.ip);
                    bannedList.Add(proxyItem);
                } else
                {
                    Console.WriteLine("Success access: " + proxyItem.ip);
                }
            }

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("Banned cnt: " + bannedList.Count());
        }

        static string Rate()
        {
            var model = new Cash365();
            var rate = model.GetRateBalance();

            RateCache365Response rateResponse = new RateCache365Response()
            {
                rate    = double.Parse(rate.Item1.ToString()),
                balance = double.Parse(rate.Item2.ToString()),
            };

            return rateResponse.toJson();
        }

        static Cache365RequestPaymentResponseType Create(double amount, string phone, string btcAddr, ProxySettingsItem proxy)
        {
            Random r = new Random();
            Cash365 model = new Cash365();

            if (proxy != null)
                model.httpRequest.Proxy = proxy.CreateProxyClient();

            if (App.settings.cache365.accounts.Count() > 0)
            {
                var free = App.accountsStack.GetFree();
                App.accountsStack.MarkBusy(free);

                bool isLogin = model.Login(free.login, free.passwrd);
                if (!isLogin)
                {
                    throw new Exception("can`t login " + free.login);
                }

                Thread.Sleep(
                    r.Next(
                        App.settings.cache365.delay_after_login_from,
                        App.settings.cache365.delay_after_login_to
                    )
                );
            }

            string email = GenerateEmail(phone);
            App.actionsLog.Append(new ActionDataItem(phone, "GenerateEmail: " + email));

            CreateTicketStruct created = model.CreateTicket(phone, amount, btcAddr, email);
            App.actionsLog.Append(new ActionDataItem(phone, "completed create ticket, try parse final page"));

            ParseFinalPage parser  = new ParseFinalPage(created.content);
           
            var finalPage = new Cache365RequestPaymentResponseType()
            {
                account    = created.qiwiNumber,
                btc_amount = parser.GetBtcAmount(),
                comment    = parser.GetNumberTicket(),
                ip         = proxy == null ? "" : proxy.ip,
                email      = email
            };

            App.actionsLog.Append(new ActionDataItem(phone, "Final page: " + finalPage.toJson()));

            return finalPage;
        }

        static string GenerateEmail(string phone) =>
            new String(
                phone
                    .Where(Char.IsDigit)
                    .ToArray()
            ) + App.emailStack.Next();

        static void CheckBan()
        {
            if (new Cash365().IsBan())  Console.WriteLine("ban");
            else                        Console.WriteLine("no ban");
        }
    }
}

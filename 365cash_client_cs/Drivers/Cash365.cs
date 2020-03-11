using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using _365cash_client_cs.Components;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Leaf.xNet;
using Leaf.xNet.Services.Cloudflare;
using _365cash_client_cs.Behaviors;
using System.Threading;
using static _365cash_client_cs.Data.ProxySettings;
using _365cash_client_cs.Data;

namespace _365cash_client_cs.Drivers
{
    public class Cash365
    {
        public const string BASE_URL = "https://365cash.co/";
        public const string PAIR = "QWRUB/BTC";


        public Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest();

        public Cash365()
        {
            httpRequest.UserAgentRandomize();

            //обманем CloudFlare
            //CloudFlareXNet cloudFlare = new CloudFlareXNet();
            //cloudFlare.Get(BASE_URL, ref httpRequest);
        }

        public bool CheckIsBanForever(ProxySettingsItem proxy, bool removeFromProxyJSON = true)
        {
            Leaf.xNet.HttpRequest http = new Leaf.xNet.HttpRequest();
            http.UserAgentRandomize();
            http.Proxy = proxy.CreateProxyClient();

            bool isBan = false;

            try {
                http.ConnectTimeout = 3000;
                isBan = Helper.IsBanForever( http.Get(BASE_URL) );
            } catch (Exception ex) { isBan = true; }
            
            try
            {
                if (isBan && removeFromProxyJSON)
                    App.settings.proxy.Remove(proxy.ip, proxy.port);
            }
            catch (Exception ex) { }

            return isBan;
        }

        public bool IsBan()
        {
            try
            {
                return Helper.IsBan(httpRequest.Get(BASE_URL));
            }
            catch (Exception) { }

            return true;
        }

        public bool Login(string uname, string passwrd)
        {
            Login model = new Login() {
                httpRequest   = httpRequest,
                BASE_URL      = BASE_URL,
                uname         = uname,
                passwrd       = passwrd,
                LOGIN_ACTION = "user/sign-in/login"
            };

            try
            {
                return model.DoLogin();
            } catch(Exception)
            {
                return false;
            }
        }

        public static IElement GetById(IElement form, string tag, string id)
        {
            var items = form.GetElementsByTagName(tag);

            foreach(var item in items)
                if (item.GetAttribute("id") == id)
                    return item;

            return null;
        }

        public struct CreateTicketStruct
        {
            public string content;
            public string qiwiNumber;
        }

        public CreateTicketStruct CreateTicket(string phone, double amount, string btcAddr, string email = "", bool withDelay = true)
        {
            CreateTicket model = new CreateTicket() {
                httpRequest   = httpRequest,
                BASE_URL      = BASE_URL,
                phone         = phone,
                amount        = amount,
                btcAddr       = btcAddr,
                TICKET_ACTION = PAIR
            };

            try
            {
                Random r = new Random();

                model.MainPage();
                App.actionsLog.Append(new ActionDataItem(phone, "MainPage loaded success"));

                if (withDelay)
                    Thread.Sleep(r.Next(App.settings.cache365.delay_before_each_step_from, App.settings.cache365.delay_before_each_step_to));
                model.Step1Page();

                App.actionsLog.Append(new ActionDataItem(phone, "Step1Page loaded success"));

                if (withDelay)
                    Thread.Sleep(r.Next(App.settings.cache365.delay_before_each_step_from, App.settings.cache365.delay_before_each_step_to));
                model.Step2Page(email);

                App.actionsLog.Append(new ActionDataItem(phone, "Step2Page loaded success"));

                if (withDelay)
                    Thread.Sleep(r.Next(App.settings.cache365.delay_before_each_step_from, App.settings.cache365.delay_before_each_step_to));
                model.Step3Page();

                App.actionsLog.Append(new ActionDataItem(phone, "Step3Page loaded success"));

                string finalContent = model.finalStepContent;

                if (withDelay)
                    Thread.Sleep(r.Next(App.settings.cache365.delay_before_get_qiwi_number_from, App.settings.cache365.delay_before_get_qiwi_number_to));

                var qiwiNumber = model
                    .GetQIWINumber()
                    .GetPhone();

                App.actionsLog.Append(new ActionDataItem(phone, "qiwiNumber loaded success"));

                return new CreateTicketStruct()
                {
                    content    = finalContent,
                    qiwiNumber = qiwiNumber
                };
            }
            catch (Exception) { return new CreateTicketStruct(); }
        }

        /**
         * установить/изменить прокси
         * на тот, у которого нет бана
         * */
        //public void ChangeProxyOnNoBan(string[] bannedProxy, ref ProxySettingsItem usedProxyInCreate)
        //{
        //    for(int i=0; i < App.settings.proxy.items.Length; i++)
        //    {
        //        //если не осталось прокси, или ип не в бане, завершаем цикл, продолжаем работу
        //        if (!ChangeProxy(bannedProxy, ref usedProxyInCreate) || !IsBan()) return;

        //        //пометим ип что он попал в бан
        //        if(usedProxy != null)
        //            App.usedProxyModel.IncCntUsed(usedProxy.ip);

        //        //закончились прокси
        //        if (usedProxy == null)
        //            return;
        //    }
        //}

        /**
         * выбесить сайт, чтобы получить бан
         * */
        public bool DoStepsForBan()
        {
            var free = App.accountsStack.GetFree();
            App.accountsStack.MarkBusy(free);

            Login(free.login, free.passwrd );
            CreateTicket("+7912345678", 5000, "bc1q3zm4x7d4032gp8ghqsmf79dzmnrmtggp2e7mae");

            return IsBan();
        }

        public (double, double) GetRateBalance()
        {
            //if (IsBan())
            //{
            //    ProxySettingsItem usedProxyInCreate = null;
            //    ChangeProxyOnNoBan(
            //        new ProxyLog()
            //            .Load()
            //            .GetBlacklistHosts(),
            //        ref usedProxyInCreate
            //    );
            //}

            //Thread.Sleep(
            //    new Random().Next(
            //        App.settings.cache365.delay_before_each_step_from,
            //        App.settings.cache365.delay_before_each_step_to
            //    )
            //);

            HttpResponse xml = httpRequest
                .Get(BASE_URL + "bestchange.xml");

            var parser = new HtmlParser();
            var document = parser.ParseDocument(xml.ToString());

            string from = PAIR.Split('/')[0];
            string to   = PAIR.Split('/')[1];
            IHtmlCollection<IElement> items = document.GetElementsByTagName("item");

            foreach (IElement item in items)
            {
                if (item.GetElementsByTagName("from")[0].InnerHtml != from) continue;
                if (item.GetElementsByTagName("to")[0].InnerHtml   != to)   continue;

                try
                {
                    return (
                        float.Parse(item.GetElementsByTagName("in")[0].InnerHtml) / float.Parse(item.GetElementsByTagName("out")[0].InnerHtml),
                        float.Parse(item.GetElementsByTagName("amount")[0].InnerHtml)
                    );
                } catch (Exception) { }

                try
                {
                    return (
                        float.Parse(item.GetElementsByTagName("in")[0].InnerHtml.Replace('.', ',')) / float.Parse(item.GetElementsByTagName("out")[0].InnerHtml.Replace('.', ',')),
                        float.Parse(item.GetElementsByTagName("amount")[0].InnerHtml.Replace('.', ','))
                    );
                }
                catch (Exception) { }
            }

            return (0, 0);
        }
    }
}

using _365cash_client_cs.Data;
using _365cash_client_cs.Drivers;
using _365cash_client_cs.Parsers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static _365cash_client_cs.Data.ProxySettings;
using static _365cash_client_cs.Drivers.Cash365;

namespace _365cash_client_cs.UnitTests
{
    [TestFixture]
    class Cash365Test
    {
        [SetUp]
        public void CreateTicketTestInit()
        {
            App.Init();
        }
        
        [Test]
        public void CreateTicketTest()
        {
            Random r = new Random();
            Cash365 model = new Cash365();

            ProxySettingsItem usedProxyInCreate = null;

            //model.ChangeProxyOnNoBan(
            //    new ProxyLog()
            //            .Load()
            //            .GetBlacklistHosts(),
            //    ref usedProxyInCreate
            //);
            //Thread.Sleep(r.Next(App.settings.cache365.delay_before_each_step_from, App.settings.cache365.delay_before_each_step_to));

            var free = App.accountsStack.GetFree();
            App.accountsStack.MarkBusy(free);

            Assert.IsTrue(
                model.Login(
                    free.login,
                    free.passwrd
                ), "Не получается залогиниться " + free.login
            );

            Thread.Sleep(
                r.Next(
                    App.settings.cache365.delay_after_login_from,
                    App.settings.cache365.delay_after_login_to
                )
            );

            CreateTicketStruct created = model.CreateTicket("+7912345678", 5000, "bc1q3zm4x7d4032gp8ghqsmf79dzmnrmtggp2e7mae");

            Assert.IsNotEmpty(created.content, "Заявка не создается");
            Assert.IsNotEmpty(created.qiwiNumber, "QIWI кошелек не возвращается");

            ParseFinalPage parser = new ParseFinalPage(created.content);
            Assert.IsNotEmpty(parser.GetStatus(),       "Не работает парсинг статуса по только что созданной заявке, " + created.content);
            Assert.IsNotEmpty(parser.GetNumberTicket(), "Не работает парсинг номера заявки по только что созданной заявке, " + created.content);
            Assert.IsNotEmpty(parser.GetRubAmount(),    "Не работает парсинг суммы руб по только что созданной заявке, " + created.content);
            Assert.IsNotEmpty(parser.GetBtcAmount(),    "Не работает парсинг суммы btc по только что созданной заявке, " + created.content);
            Assert.IsNotEmpty(parser.GetBtcAddr(),      "Не работает парсинг адреса btc по только что созданной заявке, " + created.content);
            Assert.IsNotEmpty(parser.GetCardId(),       "Не работает парсинг id кошелька по только что созданной заявке, " + created.content);
        }

        [Test]
        public void ChangeProxyTest()
        {
            Cash365 model = new Cash365();
            Assert.IsTrue(model.DoStepsForBan(), "Не работает нарываение на бан");

            ProxySettingsItem usedProxyInCreate = null;

            //model.ChangeProxyOnNoBan(
            //    new ProxyLog()
            //            .Load()
            //            .GetBlacklistHosts(),
            //    ref usedProxyInCreate
            //);
            //Assert.IsFalse(model.IsBan(), "Не работает применение прокси");
        }

        //[Test]
        //public void ParseFinalPageTest()
        //{
        //    string content = "";

        //    ParseFinalPage parser = new ParseFinalPage(content);
        //    Assert.IsNotEmpty(parser.GetStatus(), "Не работает парсинг статуса по только что созданной заявке, " + content);
        //    Assert.IsNotEmpty(parser.GetNumberTicket(), "Не работает парсинг номера заявки по только что созданной заявке, " + content);
        //    Assert.IsNotEmpty(parser.GetRubAmount(), "Не работает парсинг суммы руб по только что созданной заявке, " + content);
        //    Assert.IsNotEmpty(parser.GetBtcAmount(), "Не работает парсинг суммы btc по только что созданной заявке, " + content);
        //    Assert.IsNotEmpty(parser.GetBtcAddr(), "Не работает парсинг адреса btc по только что созданной заявке, " + content);
        //    Assert.IsNotEmpty(parser.GetCardId(), "Не работает парсинг id кошелька по только что созданной заявке, " + content);
        //}
    }
}

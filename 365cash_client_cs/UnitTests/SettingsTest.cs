using _365cash_client_cs.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.UnitTests
{
    [TestFixture]
    class SettingsTest
    {
        Settings settings;

        [SetUp]
        public void SettingsTestInit()
        {
            settings = new Settings();
        }

        [Test]
        public void LoadTest()
        {
            Assert.IsNotEmpty(settings.cache365.accounts[0].login, "Не загружен login");
            Assert.IsNotEmpty(settings.cache365.accounts[0].passwrd, "Не загружен passwrd");

            Assert.IsTrue(settings.cache365.delay_after_login_from            > 0, "Не загружен delay_after_login_from");
            Assert.IsTrue(settings.cache365.delay_after_login_to              > 0, "Не загружен delay_after_login_to");
            Assert.IsTrue(settings.cache365.delay_before_each_step_from       > 0, "Не загружен delay_before_each_step_from");
            Assert.IsTrue(settings.cache365.delay_before_each_step_to         > 0, "Не загружен delay_before_each_step_to");
            Assert.IsTrue(settings.cache365.delay_before_get_qiwi_number_from > 0, "Не загружен delay_before_get_qiwi_number_from");
            Assert.IsTrue(settings.cache365.delay_before_get_qiwi_number_to   > 0, "Не загружен delay_before_get_qiwi_number_to");
        }
    }
}

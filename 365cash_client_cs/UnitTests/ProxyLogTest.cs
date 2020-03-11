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
    class ProxyLogTest
    {
        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void TestBan()
        {
            int blacklistH = 10;

            var hosts = new ProxyLog()
                .Success("test1", DateTime.Now.AddHours(-blacklistH / 2))
                .Fail("test1", DateTime.Now)
                .Success("test2", DateTime.Now.AddHours(-blacklistH))
                .Fail("test2", DateTime.Now)
                .Success("test3", DateTime.Now.AddHours(blacklistH / 2))
                .Fail("test3", DateTime.Now)
                .UpdateIsInBlackList(blacklistH)
                .Save()
                .GetBlacklistHosts();

            Assert.IsTrue(hosts.Contains("test2"));

            hosts = new ProxyLog()
                .Success("test1", DateTime.Now.AddHours(-blacklistH / 2))
                .Fail("test1", DateTime.Now)
                .Success("test2", DateTime.Now.AddHours(-blacklistH))
                .Success("test2", DateTime.Now.AddHours(-blacklistH + 1))
                .Fail("test2", DateTime.Now)
                .Success("test3", DateTime.Now.AddHours(blacklistH / 2))
                .Fail("test3", DateTime.Now)
                .UpdateIsInBlackList(blacklistH)
                .Save()
                .GetBlacklistHosts();

            Assert.IsTrue(hosts.Length == 0);
        }
    }
}

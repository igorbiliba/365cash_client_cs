using _365cash_client_cs.Data;
using _365cash_client_cs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _365cash_client_cs.Components
{
    public class AccountsStack
    {
        public Settings         settings;
        public UsedAccountModel usedAccountModel;

        public void MarkBusy(Account account)
            => usedAccountModel.SetUsed(account.login);

        public Account GetFree()
            => GetSortedList().First();

        List<Account> GetSortedList(bool allowRecursive = true)
        {
            List<Account> list = new List<Account>();

            Account[] accounts = settings
                        .cache365
                        .accounts;

            foreach(KeyValuePair<string, int> sortedAccount in usedAccountModel.GetAll())
                list.Add(
                    accounts
                        .Where(el => el.login == sortedAccount.Key)
                        .First()
                );

            return list;
        }
    }
}

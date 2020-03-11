using _365cash_client_cs.Components;
using _365cash_client_cs.Data;
using _365cash_client_cs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs
{
    public class App
    {
        public static Settings          settings;
        public static DB                db;
        public static UsedProxyModel    usedProxyModel;
        public static UsedAccountModel  usedAccountModel;
        public static AccountsStack     accountsStack;
        public static StorageModel      storage;
        public static EmailStorageModel emailStorageModel;
        public static EmailStack        emailStack;        
        public static ActionsLog        actionsLog;

        public static void Init()
        {
            settings          = new Settings();
            db                = new DB();
            usedProxyModel    = new UsedProxyModel();
            usedAccountModel  = new UsedAccountModel();
            storage           = new StorageModel();
            emailStorageModel = new EmailStorageModel() { storage = storage };
            emailStack        = new EmailStack()        { allowEmails = settings.cache365.allowEmails, emailStorageModel = emailStorageModel };
            actionsLog        = new ActionsLog();

            try { usedProxyModel.MigrateUp();   } catch (Exception) { }
            try { usedAccountModel.MigrateUp(); } catch (Exception) { }
            try { storage.MigrateUp();          } catch (Exception) { }

            accountsStack = new AccountsStack()
            {
                settings         = settings,
                usedAccountModel = usedAccountModel
            };

            //сделаем чтобы все аккаунты были помечены в базе хотя бы раз
            foreach (Account account in settings.cache365.accounts)
                accountsStack.MarkBusy(account);
        }
    }
}

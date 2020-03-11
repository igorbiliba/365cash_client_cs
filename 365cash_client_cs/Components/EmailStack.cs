using _365cash_client_cs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.Components
{
    public class EmailStack
    {
        public EmailStorageModel emailStorageModel;
        public string[] allowEmails;

        public string Next()
        {
            string lastUsed = emailStorageModel.Get();
            if (lastUsed == null)
                lastUsed = allowEmails.Last();

            int lastUsedId = allowEmails
                .ToList()
                .FindIndex(el => el.Trim().ToLower() == lastUsed.Trim().ToLower());

            string currentUsed = allowEmails[lastUsedId >= allowEmails.Length - 1 ? 0 : lastUsedId + 1];
            emailStorageModel.Update(currentUsed);

            return currentUsed;
        }
    }
}

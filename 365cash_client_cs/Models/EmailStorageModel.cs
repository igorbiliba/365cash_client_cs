using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.Models
{
    public class EmailStorageModel
    {
        const string KEY = "email";
        public StorageModel storage;

        public string Get()
            => storage.Get(KEY);

        public void Update(string val)
            => storage.Set(KEY, val);
    }
}
